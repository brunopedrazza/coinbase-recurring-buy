using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CoinbaseRecurringBuy.Services;
using CoinbaseRecurringBuy.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<RecurringBuyConfig>(context.Configuration.GetSection("RecurringBuyConfig"));
        services.AddHttpClient();
        services.AddSingleton(x => new BlobServiceClient(context.Configuration.GetValue<string>("AzureWebJobsStorage")));
        services.AddSingleton<AllocationService>();
        services.AddSingleton<CoinbaseProService>();
    })
    .Build();

host.Run();