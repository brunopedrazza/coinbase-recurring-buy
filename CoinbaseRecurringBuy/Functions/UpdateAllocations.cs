using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;
using System.Text.Json;
using CoinbaseRecurringBuy.Models.Allocations;

namespace CoinbaseRecurringBuy.Functions;

public class UpdateAllocations(
    ILogger<UpdateAllocations> logger,
    AllocationService allocationService,
    JwtValidationService jwtValidationService)
{
    private readonly ILogger<UpdateAllocations> _logger = logger;
    private readonly AllocationService _allocationService = allocationService;
    private readonly JwtValidationService _jwtValidationService = jwtValidationService;

    [Function("UpdateAllocations")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("UpdateAllocations HTTP trigger function processed a request");

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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            RecurringBuySettings? settings;

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await MountHttpResponse(req, HttpStatusCode.BadRequest, "Invalid allocation data");
            }

            var trimmed = requestBody.TrimStart();
            if (trimmed.StartsWith("["))
            {
                var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(requestBody, jsonOptions);
                if (allocations == null)
                {
                    return await MountHttpResponse(req, HttpStatusCode.BadRequest, "Invalid allocation data");
                }

                settings = new RecurringBuySettings
                {
                    Allocations = allocations
                };
            }
            else
            {
                settings = JsonSerializer.Deserialize<RecurringBuySettings>(requestBody, jsonOptions);
            }

            if (settings == null)
            {
                return await MountHttpResponse(req, HttpStatusCode.BadRequest, "Invalid allocation data");
            }

            settings.Allocations ??= [];

            await _allocationService.UpdateSettingsAsync(settings);
            
            return await MountHttpResponse(req, HttpStatusCode.OK, new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating allocations: {Message}", ex.Message);
            return await MountHttpResponse(req, HttpStatusCode.InternalServerError, $"Error updating allocations: {ex.Message}");
        }
    }

    private async Task<HttpResponseData> MountHttpResponse(HttpRequestData req, HttpStatusCode statusCode, object data)
    {
        var response = req.CreateResponse(statusCode);
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
} 
