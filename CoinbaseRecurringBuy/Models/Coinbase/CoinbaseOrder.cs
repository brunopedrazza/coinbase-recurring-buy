using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Coinbase;

public class CoinbaseOrder
{
    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; init; } = string.Empty;

    [JsonPropertyName("product_id")]
    public string ProductId { get; init; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; init; } = string.Empty;

    [JsonPropertyName("order_configuration")]
    public OrderConfiguration OrderConfiguration { get; init; } = new();
} 