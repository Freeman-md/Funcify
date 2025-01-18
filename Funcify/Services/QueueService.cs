using Azure.Identity;
using Azure.Storage.Queues;
using Funcify.Contracts.Services;

namespace Funcify.Services;

public class QueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(QueueClient queueClient)
    {
        _queueClient = queueClient;
    }

    public async Task AddMessage(string message)
    {

        throw new NotImplementedException();
    }
}