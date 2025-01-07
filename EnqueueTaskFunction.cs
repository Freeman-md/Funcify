using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Funcify.Contracts.Services;

namespace Funcify
{
    public class EnqueueTaskFunction
    {
        private readonly ILogger<EnqueueTaskFunction> _logger;

        private readonly IQueueService _queueService;

        public EnqueueTaskFunction(ILogger<EnqueueTaskFunction> logger, IQueueService queueService)
        {
            _logger = logger;
            _queueService = queueService;
        }

        [Function("EnqueueTaskFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            _queueService.AddMessage("This is the first message");

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
