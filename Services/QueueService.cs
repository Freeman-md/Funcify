using Azure.Identity;
using Azure.Storage.Queues;
using Funcify.Contracts.Services;

namespace Funcify.Services;

public class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(string storageAccountName, string queueName)
    {
        _queueClient = new QueueClient(
                new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
    new DefaultAzureCredential()
        );
    }
}