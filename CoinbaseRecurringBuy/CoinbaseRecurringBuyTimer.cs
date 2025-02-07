using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;

namespace CoinbaseRecurringBuy;

public class CoinbaseRecurringBuyTimer(
    ILogger<CoinbaseRecurringBuyTimer> logger,
    CoinbaseProService coinbaseService,
    AllocationService allocationService)
{
    [Function("CoinbaseRecurringBuyTimer")]
    public async Task Run([TimerTrigger("%CoinbaseRecurringBuyCronExpression%", RunOnStartup = false)] TimerInfo myTimer)
    {
        logger.LogInformation("Starting recurring buy execution at: {Now}", DateTime.Now);
    
        var allocations = await allocationService.GetAllocationsAsync();
        foreach (var allocation in allocations)
        {
            try
            {
                logger.LogInformation("Placing order for {Symbol}: {Amount} USDC", allocation.Symbol, allocation.USDCAmount);
                await coinbaseService.PlaceMarketOrderAsync(allocation.Symbol, allocation.USDCAmount);
                logger.LogInformation("Successfully placed order for {Symbol}", allocation.Symbol);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error placing order for {Symbol}", allocation.Symbol);
            }
        }
    
        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next timer schedule at: {Next}", myTimer.ScheduleStatus.Next);
        }
    }
}