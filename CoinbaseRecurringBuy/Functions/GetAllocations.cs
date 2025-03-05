using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;

namespace CoinbaseRecurringBuy.Functions;

public class GetAllocations(
    ILogger<GetAllocations> logger,
    AllocationService allocationService,
    JwtValidationService jwtValidationService)
{
    private readonly ILogger<GetAllocations> _logger = logger;
    private readonly AllocationService _allocationService = allocationService;
    private readonly JwtValidationService _jwtValidationService = jwtValidationService;

    [Function("GetAllocations")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "options")] HttpRequestData req)
    {
        _logger.LogInformation("GetAllocations HTTP trigger function processed a request");

        // Handle preflight OPTIONS request
        if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var corsResponse = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(corsResponse);
            return corsResponse;
        }

        try
        {
            // Check if Authorization header exists
            if (!req.Headers.Contains("Authorization"))
            {
                _logger.LogWarning("Authorization header is missing");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCorsHeaders(unauthorizedResponse);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Missing authorization header");
                return unauthorizedResponse;
            }

            // Validate and authorize the request
            string authHeader = req.Headers.GetValues("Authorization").FirstOrDefault() ?? string.Empty;
            var (isAuthenticated, isAuthorized, principal) = await _jwtValidationService.ValidateAndAuthorizeAsync(authHeader);
            
            if (!isAuthenticated)
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCorsHeaders(unauthorizedResponse);
                await unauthorizedResponse.WriteStringAsync("Unauthorized: Invalid token");
                return unauthorizedResponse;
            }
            
            if (!isAuthorized)
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                AddCorsHeaders(forbiddenResponse);
                await forbiddenResponse.WriteStringAsync("Forbidden: You are not authorized to access this resource");
                return forbiddenResponse;
            }

            var allocations = await _allocationService.GetAllAllocationsAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response);
            await response.WriteAsJsonAsync(allocations);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations: {Message}", ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            AddCorsHeaders(response);
            await response.WriteStringAsync($"Error retrieving allocations: {ex.Message}");
            return response;
        }
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
    }
} 