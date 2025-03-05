using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Coinbase;

public class MarketMarketIoc
{
    [JsonPropertyName("quote_size")]
    public string QuoteSize { get; init; } = string.Empty;
} 