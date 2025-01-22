using System.Text;
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

        var bytes = Encoding.UTF8.GetBytes(message);

        await _queueService.AddMessage(Convert.ToBase64String(bytes));
    }
}