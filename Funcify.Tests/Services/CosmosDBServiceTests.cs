using Funcify.Contracts.Services;
using Funcify.Services;
using Funcify.Tests.Builders;
using Microsoft.Azure.Cosmos;
using Moq;

namespace Funcify.Tests.Services;

public class CosmosDBServiceTests
{
    private readonly Mock<CosmosClient> _cosmosClient;
    private readonly ICosmosDBService _cosmosDBService;
    public CosmosDBServiceTests()
    {
        _cosmosClient = new Mock<CosmosClient>();

        _cosmosDBService = new CosmosDBService(_cosmosClient.Object);
    }

    [Fact]
    public void GetContainer_WhenValidInputsAreProvided_ReturnsExpectedCosmosContainer()
    {
        #region Arrange
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                        .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                        .Returns(mockContainer.Object);
        #endregion

        #region Act
        Container container = _cosmosDBService.GetContainer("any-database", "any-container");
        #endregion

        #region Assert
        Assert.NotNull(container);
        Assert.IsAssignableFrom<Container>(container);
        #endregion
    }

    [Theory]
    [InlineData(["database", ""])]
    [InlineData(["", "container"])]
    public void GetContainer_WhenInValidInputsAreProvided_ThrowsArgumentException(string database, string container)
    {
        #region Act & Assert
        Assert.Throws<ArgumentException>(() => _cosmosDBService.GetContainer(database, container));
        #endregion
    }

    [Fact]
    public void GetContainer_WithNonExistentDatabase_ThrowsComsosException()
    {
        #region Arrange
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                        .Returns((Database)null!);
        #endregion

        #region Act & Assert
        Assert.Throws<CosmosException>(() => _cosmosDBService.GetContainer("any-database", "any-container"));
        #endregion
    }

    [Fact]
    public void GetContainer_WithNonExistentContainer_ThrowsComsosException()
    {
        #region Arrange
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                        .Returns(mockDatabase.Object);

        mockDatabase.Setup(client => client.GetContainer(It.IsAny<string>()))
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
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();
        var mockItemResponse = Mock.Of<ItemResponse<Product>>(response => response.Resource == product);


        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                        .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                        .Returns(mockContainer.Object);

        mockContainer
            .Setup(container => container.ReadItemAsync<Product>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                null,
                default))
            .ReturnsAsync(mockItemResponse);
        #endregion

        #region Act
        Product? retrievedProduct = await _cosmosDBService.GetItem<Product>("any-database", "any-container", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        #endregion

        #region Assert
        Assert.NotNull(retrievedProduct);
        Assert.Equal(product.Id, retrievedProduct.Id);
        Assert.Equal(product.Name, retrievedProduct.Name);
        Assert.Equal(product.Price, retrievedProduct.Price);
        Assert.Equal(product.Quantity, retrievedProduct.Quantity);
        #endregion
    }

    [Fact]
    public async Task GetItem_ThrowsCosmosException_WhenItemDoesNotExist()
    {
        #region Arrange
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                     .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                    .Returns(mockContainer.Object);

        mockContainer
            .Setup(container => container.ReadItemAsync<Product>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                null,
                default))
            .ThrowsAsync(new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0));
        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<CosmosException>(async () => await _cosmosDBService.GetItem<Product>(
            "any-database",
            "any-container",
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()));
        #endregion
    }


    [Theory]
    [InlineData(["1", ""])]
    [InlineData(["", "ParitionKey"])]
    public async Task GetItem_WhenInValidInputsAreProvided_ThrowsArgumentException(string id, string partitionKey)
    {
        #region Act
        await Assert.ThrowsAsync<ArgumentException>(async () => await _cosmosDBService.GetItem<Product>("any-database", "any-container", id, partitionKey));
        #endregion
    }

    [Fact]
    public async Task CreateItem_WhenValidInputsAreProvided_ShouldCreateItem()
    {
        #region Arrange
        Product product = new ProductBuilder().Build();
        var mockContainer = new Mock<Container>();
        var mockDatabase = new Mock<Database>();
        var mockItemResponse = Mock.Of<ItemResponse<Product>>(response => response.Resource == product);

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
         .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                    .Returns(mockContainer.Object);

        mockContainer.Setup(container => container.CreateItemAsync<Product>(product, null, null, default))
                        .ReturnsAsync(mockItemResponse);
        #endregion

        #region Act
        Product createdProduct = await _cosmosDBService.CreateItem<Product>("Database", "Container", product);
        #endregion

        #region Assert
        Assert.NotNull(createdProduct);
        Assert.Equal(product.Id, createdProduct.Id);
        Assert.Equal(product.Name, createdProduct.Name);
        Assert.Equal(product.Price, createdProduct.Price);
        Assert.Equal(product.Quantity, createdProduct.Quantity);
        #endregion
    }

    [Theory]
    [InlineData([null, 90.9, 9])]
    [InlineData(["", 90.9, 9])]
    [InlineData(["Product Name", null, 9])]
    [InlineData(["Product Name", 0, 9])]
    [InlineData(["Product Name", -1, 9])]
    [InlineData(["Product Name", 90.9, null])]
    [InlineData(["Product Name", 90.9, -1])]
    public async Task CreateItem_WhenInValidInputsAreProvided_ShouldThrowArgumentException(string name, decimal price, int quantity)
    {
        #region Arrange
        Product product = new ProductBuilder()
                                .WithName(name)
                                .WithPrice(price)
                                .WithQuantity(quantity)
                                .Build();
        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _cosmosDBService.CreateItem<Product>("Database", "Container", product));
        #endregion        
    }

    [Fact]
    public async Task CreateItem_WhenItemCreationFails_ShouldThrowCosmosException()
    {
        #region Arrange
        Product product = new ProductBuilder().Build();
        var mockContainer = new Mock<Container>();
        var mockDatabase = new Mock<Database>();
        var mockItemResponse = Mock.Of<ItemResponse<Product>>(response => response.Resource == product);

        _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
         .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                    .Returns(mockContainer.Object);

        mockContainer.Setup(container => container.CreateItemAsync<Product>(product, null, null, default))
                    .ThrowsAsync(new CosmosException("Item creation failed", System.Net.HttpStatusCode.InternalServerError, 0, string.Empty, 0));

        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<CosmosException>(async () => await _cosmosDBService.CreateItem<Product>("Database", "Container", product));
        #endregion  
    }
}