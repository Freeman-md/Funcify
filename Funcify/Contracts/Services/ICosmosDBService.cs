using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Funcify.Contracts.Services
{
    public interface ICosmosDBService
    {
        Container GetContainer(string databaseName, string containerName);
        Task<T?> GetItem<T>(string databaseName, string containerName, string id, string partitionKey);
        Task<T> CreateItem<T>(string databaseName, string containerName, T item);
        Task<T> UpdateItem<T>(string databaseName, string containerName, T item);
        Task<T> UpdateItemFields<T>(string databaseName, string containerName, string id, string partitionKey, Dictionary<string, object> updates);
    }
}
