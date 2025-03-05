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
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        try
        {
            var response = await blobClient.DownloadContentAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(response.Value.Content, options);
            return allocations?.Where(a => a.IsActive) ?? Enumerable.Empty<CryptoAllocation>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<CryptoAllocation>();
        }
    }

    public async Task UpdateAllocationsAsync(IEnumerable<CryptoAllocation> allocations)
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(allocations, options);
        
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task<IEnumerable<CryptoAllocation>> GetAllAllocationsAsync()
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        try
        {
            var response = await blobClient.DownloadContentAsync();
            var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(response.Value.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return allocations ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading all allocations from blob storage");
            return [];
        }
    }
} 