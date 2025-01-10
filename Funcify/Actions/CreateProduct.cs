using Funcify.Contracts.Services;
using Funcify.Services;

namespace Funcify.Actions;

public class CreateProduct {
    private readonly ICosmosDBService _cosmosDBService;
    public CreateProduct(ICosmosDBService cosmosDBService) {
        _cosmosDBService = cosmosDBService;
    }

    public async Task<Product> Invoke(Product product) {
        // TODO: Validate product

        // TODO: Create product in cosmos db
        throw new NotImplementedException();
    }
}