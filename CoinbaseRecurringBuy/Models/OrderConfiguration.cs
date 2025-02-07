using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models;

public class OrderConfiguration
{
    [JsonPropertyName("market_market_ioc")]
    public MarketMarketIoc MarketMarketIoc { get; init; } = new();
} 