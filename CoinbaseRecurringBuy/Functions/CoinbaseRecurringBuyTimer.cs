using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;
using System.Linq;

namespace CoinbaseRecurringBuy.Functions;

public class CoinbaseRecurringBuyTimer(
    ILogger<CoinbaseRecurringBuyTimer> logger,
    CoinbaseProService coinbaseService,
    AllocationService allocationService)
{
    [Function("CoinbaseRecurringBuyTimer")]
    public async Task Run([TimerTrigger("%CoinbaseRecurringBuyCronExpression%", RunOnStartup = false)] TimerInfo myTimer)
    {
        logger.LogInformation("Starting recurring buy execution at: {Now}", DateTime.Now);
    
        var settings = await allocationService.GetSettingsAsync();
        var activeAllocations = settings.Allocations.Where(a => a.IsActive).ToList();

        if (activeAllocations.Count == 0)
        {
            logger.LogInformation("No active allocations configured. Skipping recurring buy.");
            return;
        }

        var availableUsdc = await coinbaseService.GetAvailableBalanceAsync("USDC");
        logger.LogInformation("Available USDC balance: {AvailableBalance}. Minimum reserve set to: {MinimumBalance}", availableUsdc, settings.MinimumUsdcBalance);

        foreach (var allocation in activeAllocations)
        {
            try
            {
                var projectedBalance = availableUsdc - allocation.USDCAmount;
                if (projectedBalance < settings.MinimumUsdcBalance)
                {
                    logger.LogInformation(
                        "Skipping order for {Symbol}. Allocation would reduce balance to {ProjectedBalance}, below the minimum reserve of {MinimumBalance}",
                        allocation.Symbol,
                        projectedBalance,
                        settings.MinimumUsdcBalance);
                    continue;
                }

                logger.LogInformation("Placing order for {Symbol}: {Amount} USDC", allocation.Symbol, allocation.USDCAmount);
                await coinbaseService.PlaceMarketOrderAsync(allocation.Symbol, allocation.USDCAmount);
                logger.LogInformation("Successfully placed order for {Symbol}", allocation.Symbol);

                availableUsdc = projectedBalance;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error placing order for {Symbol}", allocation.Symbol);
            }
        }
    }
}
