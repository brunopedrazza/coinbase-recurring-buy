using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;
using Microsoft.Extensions.Configuration;

namespace CoinbaseRecurringBuy.Functions;

public class GetAllocations(
    ILogger<GetAllocations> logger,
    AllocationService allocationService,
    JwtValidationService jwtValidationService,
    IConfiguration configuration)
{
    private readonly ILogger<GetAllocations> _logger = logger;
    private readonly AllocationService _allocationService = allocationService;
    private readonly JwtValidationService _jwtValidationService = jwtValidationService;
    private readonly IConfiguration _configuration = configuration;

    [Function("GetAllocations")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetAllocations HTTP trigger function processed a request");

        try
        {
            // Check if Authorization header exists
            if (!req.Headers.Contains("Authorization"))
            {
                _logger.LogWarning("Authorization header is missing");
                return await MountHttpResponse(req, HttpStatusCode.Unauthorized, "Unauthorized: Missing authorization header");
            }

            // Validate and authorize the request
            string authHeader = req.Headers.GetValues("Authorization").FirstOrDefault() ?? string.Empty;
            var (isAuthenticated, isAuthorized, principal) = await _jwtValidationService.ValidateAndAuthorizeAsync(authHeader);
            
            if (!isAuthenticated)
            {
                return await MountHttpResponse(req, HttpStatusCode.Unauthorized, "Unauthorized: Invalid token");
            }
            
            if (!isAuthorized)
            {
                return await MountHttpResponse(req, HttpStatusCode.Forbidden, "Forbidden: You are not authorized to access this resource");
            }

            var allocations = await _allocationService.GetAllAllocationsAsync();
            return await MountHttpResponse(req, HttpStatusCode.OK, allocations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allocations: {Message}", ex.Message);
            return await MountHttpResponse(req, HttpStatusCode.InternalServerError, $"Error retrieving allocations: {ex.Message}");
        }
    }

    private async Task<HttpResponseData> MountHttpResponse(HttpRequestData req, HttpStatusCode statusCode, object data)
    {
        var response = req.CreateResponse(statusCode);
        AddCorsHeaders(response);
        if (data is string stringData)
        {   
            await response.WriteStringAsync(stringData);
        }
        else
        {
            await response.WriteAsJsonAsync(data);
        }
        return response;
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