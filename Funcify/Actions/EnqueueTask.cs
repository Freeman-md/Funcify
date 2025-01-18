using Funcify.Contracts.Services;

namespace Funcify.Actions;

public class EnqueueTask
{
    private readonly IQueueService _queueService;

    public EnqueueTask(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task Invoke(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Task message cannot be null or empty.", nameof(message));
        }

        await _queueService.AddMessage(message);
    }
}