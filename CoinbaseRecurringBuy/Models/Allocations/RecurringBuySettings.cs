using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Allocations;

public class RecurringBuySettings
{
    [JsonPropertyName("minimumUsdcBalance")]
    public decimal MinimumUsdcBalance { get; set; }

    [JsonPropertyName("allocations")]
    public List<CryptoAllocation> Allocations { get; set; } = [];
}
