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

    public async Task<IEnumerable<CryptoAllocation>> GetAllocationsAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.Allocations.Where(a => a.IsActive);
    }

    public async Task<RecurringBuySettings> GetSettingsAsync()
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);

        try
        {
            var response = await blobClient.DownloadContentAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var jsonContent = response.Value.Content.ToString();
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return new RecurringBuySettings();
            }

            var trimmed = jsonContent.TrimStart();
            RecurringBuySettings? settings;

            if (trimmed.StartsWith("["))
            {
                var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(jsonContent, options) ?? [];
                settings = new RecurringBuySettings
                {
                    Allocations = allocations
                };
            }
            else
            {
                settings = JsonSerializer.Deserialize<RecurringBuySettings>(jsonContent, options);
            }

            settings ??= new RecurringBuySettings();
            settings.Allocations ??= [];
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading recurring buy settings from blob storage");
            return new RecurringBuySettings();
        }
    }

    public async Task UpdateSettingsAsync(RecurringBuySettings settings)
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(settings, options);
        
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        await blobClient.UploadAsync(stream, overwrite: true);
    }
} 
