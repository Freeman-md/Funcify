using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Funcify.Tests.Functions;

public class ProductProcessingFunctionTest
{
    private readonly Mock<ILogger<ProductProcessingFunction>> _loggerMock;
    private readonly Mock<ICosmosDBService> _cosmosDBServiceMock;
    private readonly CreateProduct _createProductAction;
    private readonly ProductProcessingFunction _function;

    public ProductProcessingFunctionTest()
    {
        _loggerMock = new Mock<ILogger<ProductProcessingFunction>>();
        _cosmosDBServiceMock = new Mock<ICosmosDBService>();

        _createProductAction = new CreateProduct(_cosmosDBServiceMock.Object);

        _function = new ProductProcessingFunction(_loggerMock.Object, _createProductAction);
    }

    [Fact]
    public async Task Run_ValidProductData_ReturnsOkResult()
    {
        #region Arrange
        var product = new ProductBuilder().Build();
        var jsonProduct = JsonConvert.SerializeObject(product);
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonProduct));
        request.ContentLength = request.Body.Length;
        #endregion

        #region Act
        var result = await _function.Run(request);
        #endregion

        #region Assert
        Assert.IsType<OkObjectResult>(result);
        #endregion
    }

    [Fact]
    public async Task Run_NullProductData_ReturnsBadRequest()
    {
        #region Arrange
        var request = new DefaultHttpContext().Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        request.ContentLength = request.Body.Length;
        #endregion

        #region Act
        var result = await _function.Run(request);
        #endregion

        #region Assert
        Assert.IsType<BadRequestObjectResult>(result);
        #endregion
    }
}
