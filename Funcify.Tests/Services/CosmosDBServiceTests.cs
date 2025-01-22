using Funcify.Contracts.Services;
using Funcify.Services;
using Funcify.Tests.Builders;
using Microsoft.Azure.Cosmos;
using Moq;
using Funcify.Models;
using Xunit;

namespace Funcify.Tests.Services
{
    public class CosmosDBServiceTests
    {
        private readonly Mock<CosmosClient> _cosmosClient;
        private readonly ICosmosDBService _cosmosDBService;

        public CosmosDBServiceTests()
        {
            _cosmosClient = new Mock<CosmosClient>();
            _cosmosDBService = new CosmosDBService(_cosmosClient.Object);
        }

        #region Helper Methods

        /// <summary>
        /// Sets up a "happy path" scenario for Database/Container retrieval.
        /// Returns mocks so individual tests can override if needed.
        /// </summary>
        private (Mock<Database> MockDatabase, Mock<Container> MockContainer) SetupValidDatabaseAndContainer()
        {
            var mockDatabase = new Mock<Database>();
            var mockContainer = new Mock<Container>();

            _cosmosClient
                .Setup(c => c.GetDatabase(It.IsAny<string>()))
                .Returns(mockDatabase.Object);

            mockDatabase
                .Setup(d => d.GetContainer(It.IsAny<string>()))
                .Returns(mockContainer.Object);

            return (mockDatabase, mockContainer);
        }

        #endregion

        [Fact]
        public void GetContainer_WhenValidInputsAreProvided_ReturnsExpectedCosmosContainer()
        {
            #region Arrange
            var (_, mockContainer) = SetupValidDatabaseAndContainer();
            #endregion

            #region Act
            Container container = _cosmosDBService.GetContainer("any-database", "any-container");
            #endregion

            #region Assert
            Assert.NotNull(container);
            Assert.Same(mockContainer.Object, container);
            #endregion
        }

        [Theory]
        [InlineData("database", "")]
        [InlineData("", "container")]
        public void GetContainer_WhenInValidInputsAreProvided_ThrowsArgumentException(string database, string container)
        {
            #region Act & Assert
            Assert.Throws<ArgumentException>(() => _cosmosDBService.GetContainer(database, container));
            #endregion
        }

        [Fact]
        public void GetContainer_WithNonExistentDatabase_ThrowsCosmosException()
        {
            #region Arrange
            _cosmosClient
                .Setup(client => client.GetDatabase(It.IsAny<string>()))
                .Returns((Database)null!);
            #endregion

            #region Act & Assert
            Assert.Throws<CosmosException>(() => _cosmosDBService.GetContainer("any-database", "any-container"));
            #endregion
        }

        [Fact]
        public void GetContainer_WithNonExistentContainer_ThrowsCosmosException()
        {
            #region Arrange
            var mockDatabase = new Mock<Database>();

            _cosmosClient
                .Setup(client => client.GetDatabase(It.IsAny<string>()))
                .Returns(mockDatabase.Object);

            mockDatabase
                .Setup(db => db.GetContainer(It.IsAny<string>()))
                .Returns((Container)null!);
            #endregion

            #region Act & Assert
            Assert.Throws<CosmosException>(() => _cosmosDBService.GetContainer("any-database", "any-container"));
            #endregion
        }

        [Fact]
        public async Task GetItem_WhenValidInputsAreProvided_ReturnsItem()
        {
            #region Arrange
            Product product = new ProductBuilder().Build();
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            var mockItemResponse = Mock.Of<ItemResponse<Product>>(r => r.Resource == product);

            mockContainer
                .Setup(container => container.ReadItemAsync<Product>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ReturnsAsync(mockItemResponse);
            #endregion

            #region Act
            Product? retrievedProduct = await _cosmosDBService.GetItem<Product>(
                "any-database",
                "any-container",
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            );
            #endregion

            #region Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal(product.id, retrievedProduct.id);
            Assert.Equal(product.Name, retrievedProduct.Name);
            Assert.Equal(product.Price, retrievedProduct.Price);
            Assert.Equal(product.Quantity, retrievedProduct.Quantity);
            #endregion
        }

        [Fact]
        public async Task GetItem_ThrowsCosmosException_WhenItemDoesNotExist()
        {
            #region Arrange
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            mockContainer
                .Setup(container => container.ReadItemAsync<Product>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    null,
                    default))
                .ThrowsAsync(
                    new CosmosException(
                        "Item not found",
                        System.Net.HttpStatusCode.NotFound,
                        0,
                        string.Empty,
                        0
                    )
                );
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<CosmosException>(async () =>
                await _cosmosDBService.GetItem<Product>(
                    "any-database",
                    "any-container",
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                )
            );
            #endregion
        }

        [Theory]
        [InlineData("1", "")]
        [InlineData("", "PartitionKey")]
        public async Task GetItem_WhenInValidInputsAreProvided_ThrowsArgumentException(string id, string partitionKey)
        {
            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDBService.GetItem<Product>(
                    "any-database",
                    "any-container",
                    id,
                    partitionKey
                )
            );
            #endregion
        }

        [Fact]
        public async Task CreateItem_WithValidItem_ShouldCreateItem()
        {
            #region Arrange
            Product product = new ProductBuilder().Build();
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            var mockItemResponse = Mock.Of<ItemResponse<Product>>(response => response.Resource == product);

            mockContainer
                .Setup(c => c.CreateItemAsync(
                    product,
                    It.IsAny<PartitionKey>(),
                    null,
                    default
                ))
                .ReturnsAsync(mockItemResponse);
            #endregion

            #region Act
            Product createdProduct = await _cosmosDBService.CreateItem<Product>("Database", "Container", product);
            #endregion

            #region Assert
            Assert.NotNull(createdProduct);
            Assert.Equal(product.id, createdProduct.id);
            Assert.Equal(product.Name, createdProduct.Name);
            Assert.Equal(product.Price, createdProduct.Price);
            Assert.Equal(product.Quantity, createdProduct.Quantity);
            #endregion
        }

        [Fact]
        public async Task CreateItem_WithNullItem_ShouldThrowArgumentNullException()
        {
            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _cosmosDBService.CreateItem<Product>("Database", "Container", null!)
            );
            #endregion
        }

        [Fact]
        public async Task CreateItem_WithoutItemId_ShouldThrowArgumentException()
        {
            #region Arrange
            var item = new
            {
                Name = "Random Item"
            };
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _cosmosDBService.CreateItem("Database", "Container", item)
            );
            #endregion
        }

        [Fact]
        public async Task CreateItem_WhenItemCreationFails_ShouldThrowCosmosException()
        {
            #region Arrange
            Product product = new ProductBuilder().Build();
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            mockContainer
                .Setup(c => c.CreateItemAsync(
                    product,
                    It.IsAny<PartitionKey>(),
                    null,
                    default
                ))
                .ThrowsAsync(
                    new CosmosException(
                        "Item creation failed",
                        System.Net.HttpStatusCode.InternalServerError,
                        0,
                        string.Empty,
                        0
                    )
                );
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<CosmosException>(async () =>
                await _cosmosDBService.CreateItem<Product>("Database", "Container", product)
            );
            #endregion
        }

        [Fact]
        public async Task UpdateItem_WithValidItem_ShouldUpdateItem()
        {
            #region Arrange
            Product product = new ProductBuilder().Build();
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            var mockItemResponse = Mock.Of<ItemResponse<Product>>(response => response.Resource == product);

            mockContainer
                .Setup(c => c.UpsertItemAsync(
                    product,
                    null,
                    null,
                    default
                ))
                .ReturnsAsync(mockItemResponse);
            #endregion

            #region Act
            Product updatedProduct = await _cosmosDBService.UpdateItem<Product>("Database", "Container", product);
            #endregion

            #region Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal(product.id, updatedProduct.id);
            Assert.Equal(product.Name, updatedProduct.Name);
            Assert.Equal(product.Price, updatedProduct.Price);
            Assert.Equal(product.Quantity, updatedProduct.Quantity);
            #endregion
        }


        [Fact]
        public async Task UpdateItem_WithNullItem_ShouldThrowArgumentNullException()
        {
            #region Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _cosmosDBService.UpdateItem<Product>("Database", "Container", null!)
            );
            #endregion
        }

        [Fact]
        public async Task UpdateItem_WhenUpsertFails_ShouldThrowCosmosException()
        {
            #region Arrange
            Product product = new ProductBuilder().Build();
            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            mockContainer
                .Setup(c => c.UpsertItemAsync(
                    product,
                    null,
                    null,
                    default
                ))
                .ThrowsAsync(
                    new CosmosException(
                        "Item upsert failed",
                        System.Net.HttpStatusCode.InternalServerError,
                        0,
                        string.Empty,
                        0
                    )
                );
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<CosmosException>(async () =>
                await _cosmosDBService.UpdateItem<Product>("Database", "Container", product)
            );
            #endregion
        }

        [Fact]
        public async Task UpdateItemFields_ShouldUpdateSpecifiedFields()
        {
            #region Arrange
            string databaseName = "Database";
            string containerName = "Container";
            string itemId = "item-id";
            string partitionKey = "partition-key";

            var updates = new Dictionary<string, object>
                {
                    { "Name", "Updated Product Name" },
                    { "Price", 150.0M }
                };

            var product = new ProductBuilder()
                            .WithName(updates["Name"].ToString())
                            .WithPrice((decimal)updates["Price"])
                            .Build();

            var (_, mockContainer) = SetupValidDatabaseAndContainer();

            var patchOperations = updates.Select(update => PatchOperation.Replace($"/{update.Key}", update.Value)).ToList();

            var mockItemResponse = new Mock<ItemResponse<Product>>();
            mockItemResponse.Setup(r => r.Resource).Returns(product);

            mockContainer
                .Setup(container => container.PatchItemAsync<Product>(
                    itemId,
                    new PartitionKey(partitionKey),
                    It.IsAny<IReadOnlyList<PatchOperation>>(),
                    null,
                    default
                ))
                .ReturnsAsync(mockItemResponse.Object);
            #endregion

            #region Act
            Product updatedProduct = await _cosmosDBService.UpdateItemFields<Product>(databaseName, containerName, itemId, partitionKey, updates);
            #endregion

            #region Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal(updates["Name"], updatedProduct.Name);
            Assert.Equal(updates["Price"], updatedProduct.Price);
            #endregion
        }





    }
}
