using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;

namespace CoinbaseRecurringBuy;

public class CoinbaseRecurringBuyHttp(
    ILogger<CoinbaseRecurringBuyHttp> logger,
    CoinbaseProService coinbaseService,
    AllocationService allocationService)
{
    [Function("CoinbaseRecurringBuyHttp")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        logger.LogInformation("Starting recurring buy execution at: {Now}", DateTime.Now);

        var response = req.CreateResponse(HttpStatusCode.OK);
        var results = new List<string>();

        var allocations = await allocationService.GetAllocationsAsync();
        foreach (var allocation in allocations)
        {
            try
            {
                logger.LogInformation("Placing order for {Symbol}: {Amount} USDC", allocation.Symbol, allocation.USDCAmount);
                await coinbaseService.PlaceMarketOrderAsync(allocation.Symbol, allocation.USDCAmount);
                var message = $"Successfully placed order for {allocation.Symbol}";
                logger.LogInformation("Success: {Message}", message);
                results.Add(message);
            }
            catch (Exception ex)
            {
                var error = $"Error placing order for {allocation.Symbol}: {ex.Message}";
                logger.LogError(ex, "Error: {Error}", error);
                results.Add(error);
            }
        }

        await response.WriteAsJsonAsync(results);
        return response;
    }
} 