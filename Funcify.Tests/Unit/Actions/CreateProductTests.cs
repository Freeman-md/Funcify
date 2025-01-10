using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Tests.Builders;
using Moq;

namespace Funcify.Tests.Unit.Actions;

public class CreateProductTests
{
    private readonly Mock<ICosmosDBService> _cosmosDBService;
    private readonly CreateProduct _createProduct;

    public CreateProductTests()
    {
        _cosmosDBService = new Mock<ICosmosDBService>();

        _createProduct = new CreateProduct(_cosmosDBService.Object);
    }


    [Theory]
    [InlineData("Lenovo PC", 89.99, 9)]
    [InlineData("Mouse", 12.50, 20)]
    public async Task Invoke_WithValidInput_ShouldCreateProduct(string name, decimal price, int quantity)
    {
        #region Arrange
        Product product = new ProductBuilder()
                                .WithName(name)
                                .WithPrice(price)
                                .WithQuantity(quantity)
                                .Build();

        _cosmosDBService.Setup(service => service.CreateItem(It.IsAny<string>(), It.IsAny<string>(), product))
                        .ReturnsAsync(product);
        #endregion

        #region Act
        Product createdProduct = await _createProduct.Invoke(product);
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
    [InlineData(null, 90.9, 9)]
    [InlineData("", 90.9, 9)]
    [InlineData("Product Name", 0, 9)]
    [InlineData("Product Name", -1, 9)]
    [InlineData("Product Name", 90.9, -1)]
    public async Task Invoke_WithInvalidInput_ShouldThrowArgumentException(string name, decimal price, int quantity)
    {
        #region Arrange
        Product product = new ProductBuilder()
                                .WithName(name)
                                .WithPrice(price)
                                .WithQuantity(quantity)
                                .Build();
        #endregion

        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _createProduct.Invoke(product));
        #endregion
    }

    [Fact]
    public async Task Invoke_WithNullProduct_ShouldThrowArgumentNullException()
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _createProduct.Invoke(null!));
        #endregion
    }

    [Fact]
    public async Task Invoke_WhenServiceThrowsException_ShouldPropagateException()
    {
        #region Arrange
        Product product = new ProductBuilder().Build();

        _cosmosDBService.Setup(service => service.CreateItem(It.IsAny<string>(), It.IsAny<string>(), product))
                        .ThrowsAsync(It.IsAny<Exception>());
        #endregion

        #region Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await _createProduct.Invoke(product));
        #endregion
    }




}