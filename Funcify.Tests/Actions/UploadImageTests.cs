using System;
using Azure;
using Azure.Storage.Blobs.Models;
using Funcify.Actions;
using Funcify.Contracts.Services;
using Moq;
using Newtonsoft.Json.Serialization;

namespace Funcify.Tests.Actions;

public class UploadImageTests
{
    private readonly Mock<IBlobService> _blobService;
    private readonly UploadImage _uploadImage;

    public UploadImageTests()
    {
        _blobService = new Mock<IBlobService>();

        _uploadImage = new UploadImage(_blobService.Object);
    }

    [Fact]
    public async Task UploadImage_WithValidImageFile_ShouldUploadSuccessfully()
    {
        #region Arrange
        _blobService
            .Setup(service => service.UploadBlob(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>()
            ))
            .ReturnsAsync(Response.FromValue("", Mock.Of<Response>()));
        #endregion

        #region Act
        string blobUri;
        using (var stream = new MemoryStream())
        {
            blobUri = await _uploadImage.Invoke("Container", "File", stream);
        }
        #endregion

        #region Assert
        Assert.NotNull(blobUri);
        #endregion
    }

    [Fact]
    public async Task UploadImage_WithNullFileData_ShouldThrowArgumentException() {
        #region Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _uploadImage.Invoke("Container", "Filename", null!));
        #endregion
    }

    [Theory]
    [InlineData("", "Filename")]
    [InlineData(null, "Filename")]
    [InlineData("Container", "")]
    [InlineData("Container", null)]
    public async Task UploadImage_WithInvalidInputs_ShouldThrowArgumentException(string containerName, string fileName) {
        #region Act & Assert
        using (var stream = new MemoryStream())
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await _uploadImage.Invoke(containerName, fileName, stream));
        }
        #endregion
    }

    [Fact]
    public async Task UploadImage_WhenBlobUploadFails_ShouldThrowException() {
        #region Arrange
            _blobService
                .Setup(service => service.UploadBlob(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>()
                ))
                .ThrowsAsync(new Exception());
        #endregion
        
        #region Act & Assert
            using (var stream = new MemoryStream()) {
                await Assert.ThrowsAnyAsync<Exception>(async () => await _uploadImage.Invoke("Container", "Filename", stream));
            }
        #endregion
    }


}
