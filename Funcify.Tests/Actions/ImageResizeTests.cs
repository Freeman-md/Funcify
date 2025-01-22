using System;
using System.Threading.Tasks;
using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Services;
using Moq;

namespace Funcify.Tests.Actions;

public class ImageResizeTests
{
    private readonly Mock<IBlobService> _blobService;
    private readonly Mock<ICosmosDBService> _cosmosDBService;
    private readonly UpdateProduct _updateProduct;
    private readonly ImageResize _imageResize;

    public ImageResizeTests()
    {
        _blobService = new Mock<IBlobService>();

        _cosmosDBService = new Mock<ICosmosDBService>();

        _updateProduct = new UpdateProduct(_cosmosDBService.Object);

        _imageResize = new ImageResize(_blobService.Object, _updateProduct);
    }

    #region Helper Methods

    private (Mock<IBlobService> MockBlobService, Mock<ICosmosDBService> MockCosmosDBService) SetupServices()
    {
        _blobService.Setup(service => service.DownloadBlob(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(It.IsAny<string>());

        _cosmosDBService.Setup(service => service.UpdateItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
        .ReturnsAsync(It.IsAny<object>());


        return (_blobService, _cosmosDBService);
    }

    #endregion

    [Theory]
    [InlineData("", "valid-image.jpg")]
    [InlineData(null, "valid-image.jpg")]
    [InlineData("unprocessed-images", null)]
    [InlineData("unprocessed-images", "")]
    public async Task ImageResize_WithInvalidInputs_ThrowsArgumentException(string containerName, string blobName)
    {
        #region Arrange
        await Assert.ThrowsAsync<ArgumentException>(async () => await _imageResize.Invoke(containerName, blobName));
        #endregion
    }

    [Fact]
    public async Task ImageResize_WhenValidInputsAreProvided_ShouldCallDownloadBlobOnce()
    {
        #region Arrange
        var (mockBlobService, _) = SetupServices();
        #endregion

        #region Act
        await _imageResize.Invoke("Container", "Blob");
        #endregion

        #region Assert
        mockBlobService.Verify(service => service.DownloadBlob("Container", "Blob"), Times.Once);
        #endregion
    }

}
