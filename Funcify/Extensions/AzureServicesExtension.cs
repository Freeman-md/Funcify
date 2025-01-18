using System.Reflection.Metadata;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Funcify.Contracts.Services;
using Funcify.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions;

public static class AzureServicesExtension
{
    public static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
    {
        string storageAccountName = GetConfigurationValue(configuration, "StorageAccountName");
        string queueName = GetConfigurationValue(configuration, "QueueName");
        string cosmosDBConnectionString = GetConfigurationValue(configuration, "CosmosDBConnectionString");

        CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionString);
        BlobServiceClient blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net"),
            new DefaultAzureCredential()
        );
        QueueClient queueClient = new QueueClient(
            new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
            new DefaultAzureCredential()
        );

        services.AddSingleton<CosmosClient>(_ => cosmosClient);
        services.AddSingleton<BlobServiceClient>(_ => blobServiceClient);
        services.AddSingleton<QueueClient>(_ => queueClient);

        services.AddSingleton<IQueueService, QueueService >();
        services.AddSingleton<IBlobService, BlobService>();
        services.AddSingleton<ICosmosDBService, CosmosDBService>();

        return services;
    }

    private static string GetConfigurationValue(IConfiguration configuration, string key)
    {
        return configuration.GetValue<string>(key) ?? throw new ArgumentNullException(key);
    }
}