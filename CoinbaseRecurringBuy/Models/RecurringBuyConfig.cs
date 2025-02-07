namespace CoinbaseRecurringBuy.Models;

public class RecurringBuyConfig
{
    public string CoinbaseApiName { get; init; } = string.Empty;  // This is the API Key ID in the format "organizations/{org_id}/apiKeys/{key_id}"
    public string CoinbaseApiPrivateKey { get; init; } = string.Empty;  // This should be the EC private key in PEM format
    public string AllocationsBlobContainer { get; init; } = string.Empty;
    public string AllocationsBlobName { get; init; } = string.Empty;
} 