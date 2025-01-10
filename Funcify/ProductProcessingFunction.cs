using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Funcify.Contracts.Services;
using Funcify.Actions;
using Newtonsoft.Json;

namespace Funcify
{
    public class ProductProcessingFunction
    {
        private readonly ILogger<ProductProcessingFunction> _logger;
        private readonly CreateProduct _createProductAction;

        public ProductProcessingFunction(ILogger<ProductProcessingFunction> logger, CreateProduct createProductAction)
        {
            _logger = logger;
            _createProductAction = createProductAction;
        }

        [Function("ProductProcessingFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Product? productData = JsonConvert.DeserializeObject<Product>(requestBody);

            if (productData == null)
            {
                return new BadRequestObjectResult("Product data is null or invalid.");
            }

            await _createProductAction.Invoke(productData);


            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
