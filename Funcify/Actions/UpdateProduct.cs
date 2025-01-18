using Funcify.Contracts.Services;

namespace Funcify.Actions;

public class UpdateProduct
{
    private readonly ICosmosDBService _cosmosDBService;

    private readonly string _databaseName;
    private readonly string _containerName;

    public UpdateProduct(ICosmosDBService cosmosDBService)
    {
        _cosmosDBService = cosmosDBService;

        _databaseName = "Funcify";
        _containerName = "Products";
    }

    public async Task<Product> Invoke(Product product)
    {
        ValidateProduct(product);

        return await _cosmosDBService.UpdateItem<Product>(_databaseName, _containerName, product);
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