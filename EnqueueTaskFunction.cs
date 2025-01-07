using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Funcify
{
    public class EnqueueTaskFunction
    {
        private readonly ILogger<EnqueueTaskFunction> _logger;

        public EnqueueTaskFunction(ILogger<EnqueueTaskFunction> logger)
        {
            _logger = logger;
        }

        [Function("EnqueueTaskFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
