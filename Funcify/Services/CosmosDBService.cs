using Funcify.Contracts.Services;
using Microsoft.Azure.Cosmos;


namespace Funcify.Services;

public class CosmosDBService : ICosmosDBService
{
    private readonly CosmosClient _cosmosClient;

    public CosmosDBService(CosmosClient client)
    {
        _cosmosClient = client;
    }

    public Container GetContainer(string databaseName, string containerName)
    {
        ValidateInput(databaseName, nameof(databaseName));
        ValidateInput(containerName, nameof(containerName));

        Database database = _cosmosClient.GetDatabase(databaseName) ?? throw CreateCosmosNotFoundException(databaseName, "Database");

        Container container = database.GetContainer(containerName) ?? throw CreateCosmosNotFoundException(containerName, "Container");

        return container;
    }

    public async Task<T?> GetItem<T>(string databaseName, string containerName, string id, string partitionKey)
    {
        try
        {
            Container container = GetContainer(databaseName, containerName);

            ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    private void ValidateInput(string input, string paramName)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(paramName);
        }
    }

    private CosmosException CreateCosmosNotFoundException(string name, string entityType)
    {
        return new CosmosException($"{entityType} '{name}' was not found",
                                    System.Net.HttpStatusCode.NotFound,
                                    0,
                                    "",
                                    0);
    }
}