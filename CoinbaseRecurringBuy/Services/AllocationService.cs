using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using CoinbaseRecurringBuy.Models;
using Microsoft.Extensions.Logging;

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
            var allocations = JsonSerializer.Deserialize<List<CryptoAllocation>>(response.Value.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return allocations?.Where(a => a.IsActive) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading allocations from blob storage");
            return [];
        }
    }

    public async Task UpdateAllocationsAsync(IEnumerable<CryptoAllocation> allocations)
    {
        var blobClient = _containerClient.GetBlobClient(_blobName);
        
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, allocations);
        stream.Position = 0;
        
        await blobClient.UploadAsync(stream, overwrite: true);
    }
} 