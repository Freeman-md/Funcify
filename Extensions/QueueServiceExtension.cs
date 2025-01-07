using Funcify.Contracts.Services;
using Funcify.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions;

public static class QueueServiceExtension
{
    public static IServiceCollection AddQueueService(
        this IServiceCollection services,
        string storageAccountName,
        string queueName
    )
    {
        if (string.IsNullOrEmpty(storageAccountName))
        {
            throw new ArgumentNullException(storageAccountName);
        }

        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentNullException(queueName);
        }

        services.AddSingleton<IQueueService>(provider => new QueueService(storageAccountName, queueName));

        return services;
    }
}