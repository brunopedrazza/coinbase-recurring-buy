using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;
using System.Text.Json;
using CoinbaseRecurringBuy.Models.Allocations;
using Microsoft.Extensions.Configuration;

namespace CoinbaseRecurringBuy.Functions;

public class UpdateAllocations(
    ILogger<UpdateAllocations> logger,
    AllocationService allocationService,
    JwtValidationService jwtValidationService,
    IConfiguration configuration)
{
    private readonly ILogger<UpdateAllocations> _logger = logger;
    private readonly AllocationService _allocationService = allocationService;
    private readonly JwtValidationService _jwtValidationService = jwtValidationService;
    private readonly IConfiguration _configuration = configuration;

    [Function("UpdateAllocations")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "options")] HttpRequestData req)
    {
        _logger.LogInformation("UpdateAllocations HTTP trigger function processed a request");

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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(requestBody);

            if (allocations == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCorsHeaders(badResponse);
                await badResponse.WriteStringAsync("Invalid allocation data");
                return badResponse;
            }

            await _allocationService.UpdateAllocationsAsync(allocations);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response);
            await response.WriteAsJsonAsync(new { success = true });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating allocations: {Message}", ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            AddCorsHeaders(response);
            await response.WriteStringAsync($"Error updating allocations: {ex.Message}");
            return response;
        }
    }

    private void AddCorsHeaders(HttpResponseData response)
    {
        var allowedOrigin = _configuration.GetValue<string>("Host:AllowedOrigin") ?? "http://localhost:3000";
        response.Headers.Add("Access-Control-Allow-Origin", allowedOrigin);
        response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, x-functions-key");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
    }
} 