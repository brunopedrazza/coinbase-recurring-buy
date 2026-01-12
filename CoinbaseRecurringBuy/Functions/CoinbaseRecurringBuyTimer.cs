using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Services;

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

        var allocationSettings = await allocationService.GetAllocationSettingsAsync();
        var activeAllocations = allocationSettings.Allocations
            .Where(a => a.IsActive)
            .ToList();

        if (activeAllocations.Count == 0)
        {
            logger.LogInformation("No active allocations found. Nothing to execute.");
            return;
        }

        decimal availableUsdc;
        try
        {
            availableUsdc = await coinbaseService.GetUsdcBalanceAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve USDC balance. Skipping this run.");
            return;
        }

        var minimumUsdcBalance = allocationSettings.MinimumUsdcBalance;
        logger.LogInformation("Available USDC balance: {Available}. Configured minimum: {Minimum}.", availableUsdc, minimumUsdcBalance);

        if (availableUsdc <= minimumUsdcBalance)
        {
            logger.LogWarning("Available balance is at or below the minimum threshold. No orders will be placed.");
            return;
        }

        foreach (var allocation in activeAllocations)
        {
            var projectedBalance = availableUsdc - allocation.USDCAmount;
            if (projectedBalance < minimumUsdcBalance)
            {
                logger.LogWarning(
                    "Skipping order for {Symbol}: executing would reduce USDC balance below the minimum ({Projected} < {Minimum}).",
                    allocation.Symbol,
                    projectedBalance,
                    minimumUsdcBalance);
                continue;
            }

            try
            {
                logger.LogInformation("Placing order for {Symbol}: {Amount} USDC", allocation.Symbol, allocation.USDCAmount);
                await coinbaseService.PlaceMarketOrderAsync(allocation.Symbol, allocation.USDCAmount);
                availableUsdc = projectedBalance;
                logger.LogInformation("Successfully placed order for {Symbol}. Remaining available USDC: {Remaining}", allocation.Symbol, availableUsdc);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error placing order for {Symbol}", allocation.Symbol);
            }
        }
    }
}
