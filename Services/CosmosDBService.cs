using Funcify.Contracts.Services;
using Microsoft.Azure.Cosmos;


namespace Funcify.Services;

public class CosmosDBService : ICosmosDBService {
    private readonly CosmosClient _cosmosClient;

    public CosmosDBService(string connectionString) {
        _cosmosClient = new CosmosClient(connectionString);
    }
}