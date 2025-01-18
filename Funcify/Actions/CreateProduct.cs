using Funcify.Contracts.Services;
using Funcify.Services;

namespace Funcify.Actions;

public class CreateProduct
{
    private readonly ICosmosDBService _cosmosDBService;
    private readonly string _databaseName;
    private readonly string _containerName;
    public CreateProduct(ICosmosDBService cosmosDBService)
    {
        _cosmosDBService = cosmosDBService;

        _databaseName = "Funcify";
        _containerName = "Products";
    }

    public async Task<Product> Invoke(Product product)
    {
        ValidateProduct(product);

        Console.WriteLine(product);

        Product createdProduct = await _cosmosDBService.CreateItem<Product>(_databaseName, _containerName, product);

        return createdProduct;
    }

    private void ValidateProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.id))
            throw new ArgumentException("Product ID cannot be null or empty.", nameof(product.id));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(product.Name));

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero.", nameof(product.Price));

        if (product.Quantity < 0)
            throw new ArgumentException("Product quantity cannot be negative.", nameof(product.Quantity));
    }

}