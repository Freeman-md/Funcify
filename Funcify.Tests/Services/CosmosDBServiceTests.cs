using Funcify.Contracts.Services;
using Funcify.Services;
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
    public void GetContainer_WithValidDetails_ReturnsExpectedCosmosContainer()
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
    public void GetContainer_WithInValidDetails_ThrowsArgumentNullException(string database, string container)
    {
        #region Arrange
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

        _cosmosClient.Setup(client => client.GetDatabase(database))
                        .Returns(mockDatabase.Object);

        mockDatabase.Setup(database => database.GetContainer(container))
                        .Returns(mockContainer.Object);
        #endregion

        #region Act & Assert
            Assert.Throws<ArgumentNullException>(() => _cosmosDBService.GetContainer(database, container));
        #endregion
    }

    [Fact]
    public void GetContainer_WithNonExistentDatabase_ThrowsComsosException() {
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
    public void GetContainer_WithNonExistentContainer_ThrowsComsosException() {
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
}