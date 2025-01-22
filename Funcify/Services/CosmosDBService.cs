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
        ValidateInput(id, nameof(id));
        ValidateInput(partitionKey, nameof(partitionKey));

        Container container = GetContainer(databaseName, containerName);

        ItemResponse<T> response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

        return response.Resource;

    }

    public async Task<T> CreateItem<T>(string databaseName, string containerName, T item)
    {
        ValidateItem(item);

        Container container = GetContainer(databaseName, containerName);

        ItemResponse<T> response = await container.CreateItemAsync<T>(item, new PartitionKey("products"));

        return response.Resource;
    }

    public async Task<T> UpdateItem<T>(string databaseName, string containerName, T item)
    {
        ValidateItem(item);

        Container container = GetContainer(databaseName, containerName);

        ItemResponse<T> response = await container.UpsertItemAsync<T>(item);

        return response.Resource;
    }

    public async Task<T> UpdateItemFields<T>(string databaseName, string containerName, string id, string partitionKey, Dictionary<string, object> updates)
    {
        ValidateInput(id, nameof(id));

        Container container = GetContainer(databaseName, containerName);

        var patchOperations = updates.Select(update => PatchOperation.Replace($"/{update.Key}", update.Value)).ToList();

        Console.WriteLine($"Container: {container.Id}, Id: {id}, PartitionKey: {partitionKey}");

        ItemResponse<T> response = await container.PatchItemAsync<T>(
            id: id,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patchOperations,
            null,
            default
        );

        return response.Resource;
    }


    private void ValidateInput(string input, string paramName)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException(paramName);
        }
    }

    private void ValidateItem<T>(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var idProperty = typeof(T).GetProperty("id");
        if (idProperty == null)
        {
            throw new ArgumentException("The item must have an 'id' property.");
        }

        var idValue = idProperty.GetValue(item)?.ToString();

        if (string.IsNullOrEmpty(idValue))
        {
            throw new ArgumentException("The 'id' property must have a valid value.");
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