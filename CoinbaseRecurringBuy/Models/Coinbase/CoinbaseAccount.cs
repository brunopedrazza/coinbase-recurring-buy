using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoinbaseRecurringBuy.Models.Coinbase;

public class CoinbaseAccountsResponse
{
    [JsonPropertyName("accounts")]
    public List<CoinbaseAccount> Accounts { get; set; } = [];
}

public class CoinbaseAccount
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("available_balance")]
    public CoinbaseBalance? AvailableBalance { get; set; }
}

public class CoinbaseBalance
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = "0";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}
