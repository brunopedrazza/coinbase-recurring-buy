using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Models.Configuration;
using CoinbaseRecurringBuy.Models.Allocations;

namespace CoinbaseRecurringBuy.Services;

public class AllocationService(
    BlobServiceClient blobServiceClient, 
    IOptions<RecurringBuyConfig> config, 
    ILogger<AllocationService> logger)
{
    private readonly BlobContainerClient _containerClient = blobServiceClient.GetBlobContainerClient(config.Value.AllocationsBlobContainer);
    private readonly string _blobName = config.Value.AllocationsBlobName;

    public async Task<AllocationSettings> GetAllocationSettingsAsync()
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);

        try
        {
            var response = await blobClient.DownloadContentAsync();
            var json = response.Value.Content.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AllocationSettings();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var trimmed = json.TrimStart();
            if (trimmed.StartsWith("["))
            {
                var legacyAllocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(json, options) ?? new List<CryptoAllocation>();
                return new AllocationSettings
                {
                    Allocations = legacyAllocations
                };
            }

            var settings = JsonSerializer.Deserialize<AllocationSettings>(json, options) ?? new AllocationSettings();
            settings.Allocations ??= new List<CryptoAllocation>();
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving allocation settings from blob storage");
            return new AllocationSettings();
        }
    }

    public async Task<IEnumerable<CryptoAllocation>> GetAllocationsAsync()
    {
        var settings = await GetAllocationSettingsAsync();
        return settings.Allocations.Where(a => a.IsActive);
    }

    public async Task UpdateAllocationsAsync(AllocationSettings settings)
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var safeSettings = new AllocationSettings
        {
            MinimumUsdcBalance = settings.MinimumUsdcBalance < 0 ? 0 : settings.MinimumUsdcBalance,
            Allocations = settings.Allocations ?? new List<CryptoAllocation>()
        };
        
        var json = JsonSerializer.Serialize(safeSettings, options);
        
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    [Obsolete("Use GetAllocationSettingsAsync to retrieve both allocations and configuration.")]
    public async Task<IEnumerable<CryptoAllocation>> GetAllAllocationsAsync() =>
        (await GetAllocationSettingsAsync()).Allocations;
}
