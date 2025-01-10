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

            _cosmosDBService.Setup(service => service.CreateItem());
        #endregion
    }

}