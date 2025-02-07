using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models;

public class CoinbaseOrderResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error_response")]
    public CoinbaseOrderError? ErrorResponse { get; init; }
} 