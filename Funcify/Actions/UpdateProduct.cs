using Funcify.Contracts.Services;

namespace Funcify.Actions;

public class UpdateProduct
{
    private readonly ICosmosDBService _cosmosDBService;

    public UpdateProduct(ICosmosDBService cosmosDBService)
    {
        _cosmosDBService = cosmosDBService;
    }

    public async Task<Product> Invoke(Product product)
    {
        throw new NotImplementedException();
    }

}