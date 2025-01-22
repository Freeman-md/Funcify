using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Funcify
{
    public class ResizeImageFunction
    {
        private readonly ILogger<ResizeImageFunction> _logger;

        public ResizeImageFunction(ILogger<ResizeImageFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ResizeImageFunction))]
        public void Run([QueueTrigger("funcifyqueue", Connection = "AzureQueueStorage")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
