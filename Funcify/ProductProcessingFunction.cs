using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Funcify.Contracts.Services;
using Funcify.Actions;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace Funcify
{
    public class ProductProcessingFunction
    {
        private readonly ILogger<ProductProcessingFunction> _logger;
        private readonly CreateProduct _createProductAction;
        private readonly UploadImage _uploadImageAction;
        private readonly UpdateProduct _updateProductAction;


        public ProductProcessingFunction(
            ILogger<ProductProcessingFunction> logger,
            CreateProduct createProductAction,
            UploadImage uploadImageAction,
            UpdateProduct updateProductAction
        )
        {
            _logger = logger;
            _createProductAction = createProductAction;
            _uploadImageAction = uploadImageAction;
            _updateProductAction = updateProductAction;
        }

        [Function("ProductProcessingFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Processing request to create a product.");

                Product? productData = null;

                if (req.HasFormContentType)
                {
                    _logger.LogInformation("Processing form-data request.");

                    var form = await req.ReadFormAsync();

                    // Extract product details
                    var name = form["Name"].ToString();
                    var price = decimal.TryParse(form["Price"], out var parsedPrice) ? parsedPrice : 0;
                    var quantity = int.TryParse(form["Quantity"], out var parsedQuantity) ? parsedQuantity : 0;

                    if (string.IsNullOrWhiteSpace(name) || price <= 0 || quantity < 0)
                    {
                        return new BadRequestObjectResult("Invalid product data in form submission.");
                    }

                    //TODO: Handle File Upload
                    if (form.Files.Count > 0)
                    {
                        var file = form.Files[0];

                        if (!IsValidImage(file))
                        {
                            return new BadRequestObjectResult("Invalid file format. Please upload a valid image.");
                        }

                        using var stream = file.OpenReadStream();
                        var blobUri = await _uploadImageAction.Invoke("unprocessed-images", file.FileName, stream);

                        productData = new Product
                        {
                            Name = name,
                            Price = price,
                            Quantity = quantity,
                            UnprocessedImageUrl = blobUri
                        };
                    }
                    else
                    {
                        return new BadRequestObjectResult("No file uploaded. Please upload an image file.");
                    }
                }
                else if (req.ContentType == "application/json")
                {
                    _logger.LogInformation("Processing JSON request.");

                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    productData = JsonConvert.DeserializeObject<Product>(requestBody);

                    if (productData == null)
                    {
                        return new BadRequestObjectResult("Product data is null or invalid.");
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Unsupported content type. Please send JSON or form-data.");
                }

                Product createdProduct = await _createProductAction.Invoke(productData);

                _logger.LogInformation($"Product created successfully: {createdProduct}");
                return new OkObjectResult(new
                {
                    Message = "Product created successfully",
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

        private bool IsValidImage(IFormFile file)
        {
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(fileExtension) && permittedExtensions.Contains(fileExtension);
        }

    }
}
