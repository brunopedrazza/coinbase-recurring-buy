namespace CoinbaseRecurringBuy.Models;

public class CryptoAllocation
{
    public string Symbol { get; set; } = "";
    public decimal USDCAmount { get; set; }
    public bool IsActive { get; set; } = true;
} 