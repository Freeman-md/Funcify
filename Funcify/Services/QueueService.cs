using Azure.Identity;
using Azure.Storage.Queues;
using Funcify.Contracts.Services;

namespace Funcify.Services;

public class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(string storageAccountName, string queueName)
    {
        string uri = $"https://{storageAccountName}.queue.core.windows.net/{queueName}";

        _queueClient = new QueueClient(
                new Uri(uri),
    new DefaultAzureCredential()
        );
    }

    public async Task AddMessage(string message)
    {
        await _queueClient.SendMessageAsync(message);
    }
}