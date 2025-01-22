using System;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Funcify.Actions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Funcify
{
    public class ResizeImageFunction
    {
        private readonly ILogger<ResizeImageFunction> _logger;
        private readonly ImageResize _imageResize;

        public ResizeImageFunction(ILogger<ResizeImageFunction> logger, ImageResize imageResize)
        {
            _logger = logger;

            _imageResize = imageResize;
        }

        [Function($"ResizeImageFunction")]
        public async Task Run([QueueTrigger("funcifyqueue", Connection = "Storage")] QueueMessage message)
        {
             try
            {
                _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

                var queueMessage = JsonConvert.DeserializeObject<ProductImageProcessingMessage>(message.MessageText);
                
                if (queueMessage == null)
                {
                    _logger.LogError("Failed to deserialize the queue message.");
                    return;
                }

                _logger.LogInformation($"Processing product with ID: {queueMessage.ProductId}, Image URL: {queueMessage.UnprocessedImageUrl} and File Name: {queueMessage.FileName}");

                await _imageResize.Invoke("processed-images", queueMessage.ProductId, "products", queueMessage.FileName);  
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Missing required product data.");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid product input data.");
                throw;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Resource not found in Cosmos DB.");
                throw;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "An error occurred while interacting with Cosmos DB.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                throw;
            }
        }
    }
}
