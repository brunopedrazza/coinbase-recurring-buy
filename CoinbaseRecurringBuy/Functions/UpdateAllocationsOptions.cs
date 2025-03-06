using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CoinbaseRecurringBuy.Functions;

public class UpdateAllocationsOptions(
    ILogger<UpdateAllocationsOptions> logger,
    IConfiguration configuration)
{
    private readonly ILogger<UpdateAllocationsOptions> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    [Function("UpdateAllocationsOptions")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "UpdateAllocations")] HttpRequestData req)
    {
        _logger.LogInformation("UpdateAllocations OPTIONS request processed");

        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(req, response);
        return response;
    }

    private void AddCorsHeaders(HttpRequestData req, HttpResponseData response)
    {
        // Get the origin from the request
        var requestOrigin = req.Headers.GetValues("Origin").FirstOrDefault();
        
        // Get allowed origins from configuration
        var configuredOrigins = _configuration.GetValue<string>("Host:AllowedOrigin") ?? "http://localhost:3000";
        var allowedOrigins = configuredOrigins.Split(',');
        
        // If the request origin is in our allowed list, return it in the header
        if (!string.IsNullOrEmpty(requestOrigin) && allowedOrigins.Contains(requestOrigin))
        {
            response.Headers.Add("Access-Control-Allow-Origin", requestOrigin);
        }
        else
        {
            // Default to the first configured origin
            response.Headers.Add("Access-Control-Allow-Origin", allowedOrigins[0]);
        }
        
        response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, x-functions-key");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
        response.Headers.Add("Access-Control-Max-Age", "86400");
    }
} 