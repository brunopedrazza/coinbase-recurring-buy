using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models;

public class MarketMarketIoc
{
    [JsonPropertyName("quote_size")]
    public string QuoteSize { get; init; } = string.Empty;
} 