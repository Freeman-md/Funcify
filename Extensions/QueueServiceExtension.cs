using Funcify.Contracts.Services;
using Funcify.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions;

public static class QueueServiceExtension
{


    public static IServiceCollection AddQueueService(this IServiceCollection services, IConfiguration configuration)
    {
        string storageAccountName = configuration.GetValue<string>("StorageAccountName") 
        ?? throw new ArgumentNullException(nameof(storageAccountName));
        string queueName = configuration.GetValue<string>("QueueName") 
        ?? throw new ArgumentNullException(nameof(queueName));

        services.AddSingleton<IQueueService>(provider => new QueueService(
            storageAccountName,
            queueName
        ));

        return services;
    }

}