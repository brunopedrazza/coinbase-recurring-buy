using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
        AddCorsHeaders(response);
        return response;
    }

    private void AddCorsHeaders(HttpResponseData response)
    {
        var allowedOrigin = _configuration.GetValue<string>("Host:AllowedOrigin") ?? "http://localhost:3000";
        response.Headers.Add("Access-Control-Allow-Origin", allowedOrigin);
        response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, x-functions-key");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
        response.Headers.Add("Access-Control-Max-Age", "86400");
    }
} 