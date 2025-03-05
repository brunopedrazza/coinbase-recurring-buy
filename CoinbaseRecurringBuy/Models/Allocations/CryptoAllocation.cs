using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Allocations;

public class CryptoAllocation
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("usdcAmount")]
    public decimal USDCAmount { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
} 