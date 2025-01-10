using Funcify.Contracts.Services;
using Funcify.Services;

namespace Funcify.Actions;

public class CreateProduct {
    private readonly ICosmosDBService _cosmosDBService;
    public CreateProduct(ICosmosDBService cosmosDBService) {
        _cosmosDBService = cosmosDBService;
    }

    public async Task Invoke(string json) {
        // TODO: Deserialize json into product object

        // TODO: Validate product

        // TODO: Create product in cosmos db
        throw new NotImplementedException();
    }
}