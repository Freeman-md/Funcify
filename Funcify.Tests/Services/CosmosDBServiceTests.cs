using Funcify.Contracts.Services;
using Funcify.Services;
using Microsoft.Azure.Cosmos;
using Moq;

namespace Funcify.Tests.Services;

public class CosmosDBServiceTests {
    private readonly Mock<CosmosClient> _cosmosClient;
    private readonly ICosmosDBService _cosmosDBService;
    public CosmosDBServiceTests() {
        _cosmosClient = new Mock<CosmosClient>();

        _cosmosDBService = new CosmosDBService(_cosmosClient.Object);
    }

    [Fact]
    public void GetContainer_ShouldReturnValidCosmosContainer() {
        #region Act
        var mockDatabase = new Mock<Database>();
        var mockContainer = new Mock<Container>();

            _cosmosClient.Setup(client => client.GetDatabase(It.IsAny<string>()))
                            .Returns(mockDatabase.Object);

            mockDatabase.Setup(database => database.GetContainer(It.IsAny<string>()))
                            .Returns(mockContainer.Object);
        #endregion

        #region Act
            
        #endregion
    }
} 