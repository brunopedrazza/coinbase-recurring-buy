using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CoinbaseRecurringBuy.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using CoinbaseRecurringBuy.Models.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<RecurringBuyConfig>(context.Configuration.GetSection("RecurringBuyConfig"));
        services.Configure<AzureAdB2cConfig>(context.Configuration.GetSection("AzureAdB2cConfig"));
        services.AddHttpClient();
        services.AddSingleton(x => new BlobServiceClient(context.Configuration.GetValue<string>("AzureWebJobsStorage")));
        services.AddSingleton<AllocationService>();
        services.AddSingleton<CoinbaseProService>();
        services.AddSingleton<JwtValidationService>();
    })
    .Build();

host.Run();