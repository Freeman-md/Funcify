using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Tests.Builders;
using Moq;

namespace Funcify.Tests.Unit.Actions;

public class CreateProductTests {
    private readonly Mock<ICosmosDBService> _cosmosDBService;
    private readonly CreateProduct _createProduct;

    public CreateProductTests() {
        _cosmosDBService = new Mock<ICosmosDBService>();

        _createProduct = new CreateProduct(_cosmosDBService.Object);
    }


    [Theory]
    [InlineData(["Lenovo PC", 89.99, 9])]
    public void Invoke_WithValidData_ShouldCreateProduct(string name, decimal price, int quantity) {
        #region Act
            Product product = new ProductBuilder()
                                    .WithName(name)
                                    .WithPrice(price)
                                    .WithQuantity(quantity)
                                    .Build();

            // _cosmosDBService.Setup(service => service.CreateItem());
        #endregion
    }

    // [Theory]
    // [InlineData([null, 90.9, 9])]
    // [InlineData(["", 90.9, 9])]
    // [InlineData(["Product Name", null, 9])]
    // [InlineData(["Product Name", 0, 9])]
    // [InlineData(["Product Name", -1, 9])]
    // [InlineData(["Product Name", 90.9, null])]
    // [InlineData(["Product Name", 90.9, -1])]
    // public async Task Invoke_WhenInValidInputsAreProvided_ShouldThrowArgumentException(string name, decimal price, int quantity)
    // {
    //     #region Arrange
    //     Product product = new ProductBuilder()
    //                             .WithName(name)
    //                             .WithPrice(price)
    //                             .WithQuantity(quantity)
    //                             .Build();
    //     #endregion

    //     #region Act & Assert
    //     await Assert.ThrowsAsync<ArgumentException>(async () => await _createProduct.Invoke(product));
    //     #endregion        
    // }

}