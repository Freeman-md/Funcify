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

        CosmosClient client = new CosmosClient(cosmosDBConnectionString);

        services.AddSingleton<IQueueService>(provider => new QueueService(storageAccountName, queueName));
        services.AddSingleton<IBlobService>(provider => new BlobService(storageAccountName));
        services.AddSingleton<ICosmosDBService, CosmosDBService>();

        return services;
    }

    private static string GetConfigurationValue(IConfiguration configuration, string key)
    {
        return configuration.GetValue<string>(key) ?? throw new ArgumentNullException(key);
    }
}