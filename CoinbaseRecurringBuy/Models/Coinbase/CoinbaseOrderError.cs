using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Coinbase;

public class CoinbaseOrderError
{
    [JsonPropertyName("error")]
    public string Error { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("error_details")]
    public string ErrorDetails { get; init; } = string.Empty;

    [JsonPropertyName("preview_failure_reason")]
    public string PreviewFailureReason { get; init; } = string.Empty;
} 