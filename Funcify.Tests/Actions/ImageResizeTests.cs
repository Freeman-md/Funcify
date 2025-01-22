using System;
using System.IO;
using System.Threading.Tasks;
using Funcify.Actions;
using Funcify.Contracts.Services;
using Funcify.Services;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Funcify.Tests.Actions
{
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
        private (string, string) CreateDummyImage()
        {
            string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "testImages");
            Directory.CreateDirectory(tempDir);

            string dummyImagePath = Path.Combine(tempDir, "dummy-image.jpg");

            using (var image = new Image<Rgba32>(10, 10))
            {
                image[0, 0] = new Rgba32(255, 0, 0);
                image.SaveAsJpeg(dummyImagePath);
            }

            return (dummyImagePath, tempDir);
        }

        private (Mock<IBlobService> MockBlobService, Mock<ICosmosDBService> MockCosmosDBService) SetupServices()
        {
            var (dummyImagePath, _) = CreateDummyImage();

            _blobService.Setup(service => service.DownloadBlob(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(dummyImagePath);

            _blobService.Setup(service => service.UploadBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync("/upload-path");

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
        public async Task ImageResize_ShouldCallDownloadBlobOnce_InBlobService()
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

        [Fact]
        public async Task ImageResize_ShouldCallUploadBlobOnce_InBlobService()
        {
            #region Arrange
            var (mockBlobService, _) = SetupServices();
            #endregion

            #region Act
            await _imageResize.Invoke("Container", "Blob");
            #endregion

            #region Assert
            mockBlobService.Verify(service => service.UploadBlob("Container", "Blob", It.IsAny<Stream>()), Times.Once);
            #endregion
        }

        public void Dispose()
        {
            string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "testImages");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
