using Microsoft.Azure.Cosmos;

namespace Funcify.Contracts.Services;

public interface ICosmosDBService {
    public Container GetContainer(string databaseName, string containerName);

    public Task<T?> GetItem<T>(string databaseName, string containerName, string id, string partitionKey);
}