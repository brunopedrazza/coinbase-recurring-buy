namespace CoinbaseRecurringBuy.Models.Configuration;

public class AzureAdB2cConfig
{
    public string Tenant { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string AuthorizedUsers { get; set; } = string.Empty;
} 