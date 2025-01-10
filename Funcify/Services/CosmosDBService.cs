using Funcify.Contracts.Services;
using Microsoft.Azure.Cosmos;


namespace Funcify.Services;

public class CosmosDBService : ICosmosDBService {
    private readonly CosmosClient _cosmosClient;

    public CosmosDBService(CosmosClient client) {
        _cosmosClient = client;
    }

    public Container GetContainer(string databaseName, string containerName) {
        Database database = _cosmosClient.GetDatabase(databaseName);
        
        return database.GetContainer(containerName);
    }

    public async Task<T?> GetItem<T>(string databaseName, string containerName, string id, string partitionKey) {
        Container container = GetContainer(databaseName, containerName);

        try
        {
            ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return default;
        }
    }
}