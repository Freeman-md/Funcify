using Funcify.Actions;
using Funcify.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Funcify
{
    public class ProductProcessingFunction
    {
        private readonly ILogger<ProductProcessingFunction> _logger;
        private readonly CreateProduct _createProductAction;
        private readonly UploadImage _uploadImageAction;
        private readonly UpdateProduct _updateProductAction;
        private readonly EnqueueTask _enqueueTaskAction;

        private static readonly string[] PermittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public ProductProcessingFunction(
            ILogger<ProductProcessingFunction> logger,
            CreateProduct createProductAction,
            UploadImage uploadImageAction,
            UpdateProduct updateProductAction,
            EnqueueTask enqueueTaskAction)
        {
            _logger = logger;
            _createProductAction = createProductAction;
            _uploadImageAction = uploadImageAction;
            _updateProductAction = updateProductAction;
            _enqueueTaskAction = enqueueTaskAction;
        }

        [Function("ProductProcessingFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("Received a request to process a product.");

            try
            {
                var productData = await ParseAndValidateRequestAsync(req);

                if (productData.RequiresImageUpload)
                {
                    var blobUri = await UploadProductImageAsync(productData.ImageFile);
                    productData.UnprocessedImageUrl = blobUri;
                    productData.FileName = productData.ImageFile.FileName;
                }

                var createdProduct = await CreateProductAsync(productData);
                _logger.LogInformation($"Product created successfully: {createdProduct?.id}");

                if (!string.IsNullOrEmpty(createdProduct.UnprocessedImageUrl))
                {
                    await EnqueueImageProcessingAsync(createdProduct);
                }

                return new OkObjectResult(new
                {
                    Message = "Product processed successfully",
                    Product = createdProduct
                });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Missing required product data.");
                return new BadRequestObjectResult(new { Error = "Invalid request", Details = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid product input data.");
                return new BadRequestObjectResult(new { Error = "Invalid product data", Details = ex.Message });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Resource not found in Cosmos DB.");
                return new NotFoundObjectResult(new { Error = "Resource not found", Details = ex.Message });
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "An error occurred while interacting with Cosmos DB.");
                return new ObjectResult(new { Error = "Cosmos DB error", Details = ex.Message })
                {
                    StatusCode = (int)ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return new ObjectResult(new { Error = "Internal server error", Details = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        #region Action Methods

        private async Task<Product> ParseAndValidateRequestAsync(HttpRequest req)
        {
            if (req.HasFormContentType)
            {
                _logger.LogInformation("Processing form-data request.");
                return await ProcessFormDataAsync(req);
            }
            else if (req.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing JSON request.");
                return await ProcessJsonAsync(req);
            }
            else
            {
                throw new ArgumentException("Unsupported content type. Please send JSON or form-data.");
            }
        }

        private async Task<Product> ProcessFormDataAsync(HttpRequest req)
        {
            var form = await req.ReadFormAsync();

            var name = form["Name"].ToString();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.");

            if (!decimal.TryParse(form["Price"], out var price) || price <= 0)
                throw new ArgumentException("Invalid price value.");

            if (!int.TryParse(form["Quantity"], out var quantity) || quantity < 0)
                throw new ArgumentException("Invalid quantity value.");

            if (form.Files.Count == 0)
                throw new ArgumentException("No file uploaded. Please upload an image file.");

            var file = form.Files[0];
            if (!IsValidImage(file))
                throw new ArgumentException("Invalid file format. Please upload a valid image.");

            return new Product
            {
                id = Guid.NewGuid().ToString(),
                Name = name,
                Price = price,
                Quantity = quantity,
                RequiresImageUpload = true,
                ImageFile = file
            };
        }

        private async Task<Product> ProcessJsonAsync(HttpRequest req)
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();

            var product = JsonConvert.DeserializeObject<Product>(requestBody);
            if (product == null)
                throw new ArgumentException("Product data is null or invalid.");

            ValidateProductData(product);
            return product;
        }

        private void ValidateProductData(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name is required.");

            if (product.Price <= 0)
                throw new ArgumentException("Price must be greater than zero.");

            if (product.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.");
        }

        private async Task<string> UploadProductImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var blobUri = await _uploadImageAction.Invoke("unprocessed-images", file.FileName, stream);
            _logger.LogInformation($"Image uploaded successfully: {blobUri}");
            return blobUri;
        }

        private async Task<Product> CreateProductAsync(Product product)
        {
            var createdProduct = await _createProductAction.Invoke(product);
            if (createdProduct == null)
                throw new Exception("Failed to create product.");
            return createdProduct;
        }

        private async Task EnqueueImageProcessingAsync(Product product)
        {
            var processingMessage = new ProductImageProcessingMessage(
                product.id,
                product.UnprocessedImageUrl,
                product.FileName
            );

            var messageBody = JsonConvert.SerializeObject(processingMessage);
            await _enqueueTaskAction.Invoke(messageBody);
            _logger.LogInformation($"Enqueued task for processing image: {product.UnprocessedImageUrl}");
        }

        #endregion

        #region Helper Methods

        private bool IsValidImage(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return PermittedExtensions.Contains(fileExtension);
        }

        #endregion
    }
}
