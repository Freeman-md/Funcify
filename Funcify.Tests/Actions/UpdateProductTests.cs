using System;
using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Tests.Builders;
using Moq;

namespace Funcify.Tests.Actions;

public class UpdateActionTests
{
    private readonly Mock<ICosmosDBService> _cosmosDBService;

    private readonly UpdateProduct _updateProduct;

    public UpdateActionTests()
    {
        _cosmosDBService = new Mock<ICosmosDBService>();

        _updateProduct = new UpdateProduct(_cosmosDBService.Object);
    }

    [Fact]
    public async Task Invoke_WithValidInput_ShouldUpdateProduct()
    {
        #region Arrange
        Product product = new ProductBuilder()
            .Build();

        _cosmosDBService.Setup(service => service.UpdateItem(It.IsAny<string>(), It.IsAny<string>(), product))
                        .ReturnsAsync(product);

        var updateProduct = new UpdateProduct(_cosmosDBService.Object);
        #endregion

        #region Act
        Product updatedProduct = await updateProduct.Invoke(product);
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
    public async Task Invoke_WithNullProduct_ShouldThrowArgumentNullException()
    {
        #region Arrange
        var updateProduct = new UpdateProduct(_cosmosDBService.Object);
        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await updateProduct.Invoke(null!));
        #endregion
    }

    [Theory]
    [InlineData(null, 50.0, 10)]  // Missing Name
    [InlineData("Valid Name", 0.0, 10)]  // Invalid Price
    [InlineData("Valid Name", -1.0, 10)]  // Negative Price
    [InlineData("Valid Name", 50.0, -1)]  // Negative Quantity
    public async Task Invoke_WithInvalidInput_ShouldThrowArgumentException(string name, decimal price, int quantity)
    {
        #region Arrange
        Product product = new ProductBuilder()
            .WithId(Guid.NewGuid().ToString())
            .WithName(name)
            .WithPrice(price)
            .WithQuantity(quantity)
            .Build();

        var updateProduct = new UpdateProduct(_cosmosDBService.Object);
        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await updateProduct.Invoke(product));
        #endregion
    }

    [Fact]
    public async Task Invoke_WhenServiceThrowsException_ShouldPropagateException()
    {
        #region Arrange
        Product product = new ProductBuilder().Build();

        _cosmosDBService.Setup(service => service.UpdateItem(It.IsAny<string>(), It.IsAny<string>(), product))
                        .ThrowsAsync(new Exception("Database error"));

        var updateProduct = new UpdateProduct(_cosmosDBService.Object);
        #endregion

        #region Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => await updateProduct.Invoke(product));
        Assert.Equal("Database error", exception.Message);
        #endregion
    }

}
