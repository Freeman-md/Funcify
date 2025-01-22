using Funcify.Contracts.Services;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Funcify.Services
{
    public class CosmosDBService : ICosmosDBService
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosDBService(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        public Container GetContainer(string databaseName, string containerName)
        {
            ValidateInput(databaseName, nameof(databaseName));
            ValidateInput(containerName, nameof(containerName));

            Database database = _cosmosClient.GetDatabase(databaseName)
                ?? throw CreateCosmosNotFoundException(databaseName, "Database");

            Container container = database.GetContainer(containerName)
                ?? throw CreateCosmosNotFoundException(containerName, "Container");

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

            // The PartitionKey (e.g., "products") could be dynamic depending on your schema
            ItemResponse<T> response = await container.CreateItemAsync(item, new PartitionKey("products"));
            return response.Resource;
        }

        public async Task<T> UpdateItem<T>(string databaseName, string containerName, T item)
        {
            ValidateItem(item);

            Container container = GetContainer(databaseName, containerName);
            ItemResponse<T> response = await container.UpsertItemAsync(item);

            return response.Resource;
        }

        public async Task<T> UpdateItemFields<T>(string databaseName, string containerName, string id, string partitionKey, Dictionary<string, object> updates)
        {
            ValidateInput(id, nameof(id));

            Container container = GetContainer(databaseName, containerName);

            var patchOperations = updates
                .Select(u => PatchOperation.Replace($"/{u.Key}", u.Value))
                .ToList();

            ItemResponse<T> response = await container.PatchItemAsync<T>(
                id: id,
                partitionKey: new PartitionKey(partitionKey),
                patchOperations: patchOperations
            );

            return response.Resource;
        }

        #region Private Helper Methods

        private void ValidateInput(string input, string paramName)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
            }
        }

        private void ValidateItem<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var idProperty = typeof(T).GetProperty("id");
            if (idProperty == null)
                throw new ArgumentException("The item must have an 'id' property.");

            var idValue = idProperty.GetValue(item)?.ToString();
            if (string.IsNullOrEmpty(idValue))
                throw new ArgumentException("The 'id' property must have a valid value.");
        }

        private CosmosException CreateCosmosNotFoundException(string name, string entityType)
        {
            return new CosmosException(
                $"{entityType} '{name}' was not found",
                System.Net.HttpStatusCode.NotFound,
                0,
                string.Empty,
                0
            );
        }

        #endregion
    }
}
