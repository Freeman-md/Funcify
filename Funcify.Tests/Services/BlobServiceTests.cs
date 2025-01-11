using System;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Funcify.Contracts.Services;
using Funcify.Services;
using Moq;

namespace Funcify.Tests.Services;

public class BlobServiceTests
{
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly IBlobService _blobService;

    public BlobServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();

        _blobService = new BlobService(_mockBlobServiceClient.Object);
    }

    [Fact]
    public async Task CreateContainer_IfNotExists_WithValidContainerName_ShouldCreateContainer()
    {
        #region Arrange
        string containerName = "unprocessed-images";

        Mock<BlobContainerClient> mockBlobContainerClient = new Mock<BlobContainerClient>();

        _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                            .Returns(mockBlobContainerClient.Object);

        mockBlobContainerClient.Setup(client => client.ExistsAsync(default))
                                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        mockBlobContainerClient
            .Setup(client => client.CreateIfNotExistsAsync(default, null, null, default))
            .ReturnsAsync(Response.FromValue(Mock.Of<BlobContainerInfo>(), Mock.Of<Response>()));
        #endregion

        #region Act
        await _blobService.CreateContainer(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Once);
        #endregion
    }

    [Fact]
    public async Task CreateContainer_WithExistingContainer_ShouldNotCreateContainer()
    {
        #region Arrange
        string containerName = "unprocessed-images";

        Mock<BlobContainerClient> mockBlobContainerClient = new Mock<BlobContainerClient>();

        _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                            .Returns(mockBlobContainerClient.Object);

        mockBlobContainerClient.Setup(client => client.ExistsAsync(default))
                                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));
        #endregion

        #region Act
        await _blobService.CreateContainer(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Never);
        #endregion
    }

    [Theory]
    [InlineData([""])]
    [InlineData([null])]
    public async Task CreateContainer_WithInvalidContainerName_ShouldThrowArgumentException(string containerName)
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _blobService.CreateContainer(containerName));
        #endregion
    }

    [Fact]
    public async Task CreateContainer_WhenBlobServiceClientThrows_ShouldPropagateException()
    {
        #region Arrange
        _mockBlobServiceClient
                .Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Throws(It.IsAny<Exception>);
        #endregion

        #region Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () => await _blobService.CreateContainer("unprocessed-images"));

        _mockBlobServiceClient.Verify(client => client.GetBlobContainerClient("unprocessed-images"), Times.Once);
        #endregion
    }

    [Fact]
    public async Task UploadBlob_WithValidInput_ShouldUploadBlobSuccessfully()
    {
        #region Arrange
        var containerName = "valid-container";
        var blobName = "valid-blob.txt";
        var data = new MemoryStream(Encoding.UTF8.GetBytes("Sample blob data"));

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();

        _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                          .Returns(mockBlobContainerClient.Object);

        mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName))
                               .Returns(mockBlobClient.Object);

        mockBlobClient.Setup(blob => blob.UploadAsync(It.IsAny<Stream>(), true, default))
                      .ReturnsAsync(Response.FromValue(Mock.Of<BlobContentInfo>(), Mock.Of<Response>()));
        #endregion

        #region Act
        await _blobService.UploadBlob(containerName, blobName, data);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(container => container.GetBlobClient(blobName), Times.Once);
        mockBlobClient.Verify(blob => blob.UploadAsync(data, true, default), Times.Once);
        #endregion
    }

    [Theory]
    [InlineData(["", "blob-file.txt"])]
    [InlineData([null, "blob-file.txt"])]
    [InlineData(["unprocessed-image", ""])]
    [InlineData(["unprocessed-image", null])]
    public async Task UploadBlob_WithValidInputs_ShouldThrowArgumentException(string containerName, string blobName)
    {
        #region Arrange
        Mock<BlobContainerClient> mockBlobContainerClient = new Mock<BlobContainerClient>();
        var data = new MemoryStream(Encoding.UTF8.GetBytes("Sample blob data"));
        #endregion

        #region Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _blobService.UploadBlob(containerName, blobName, data));

        _mockBlobServiceClient.Verify(service => service.GetBlobContainerClient(containerName), Times.Never);
        mockBlobContainerClient.Verify(container => container.GetBlobClient(blobName), Times.Never);
        #endregion
    }

    [Fact]
    public async Task UploadBlob_WithEmptyData_ShouldThrowArgumentException()
    {
        #region Arrange
        Mock<BlobContainerClient> mockBlobContainerClient = new Mock<BlobContainerClient>();

        var containerName = "valid-container";
        var blobName = "valid-blob.txt";
        #endregion

        #region Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _blobService.UploadBlob(containerName, blobName, null!));

        _mockBlobServiceClient.Verify(service => service.GetBlobContainerClient(containerName), Times.Never);
        mockBlobContainerClient.Verify(container => container.GetBlobClient(blobName), Times.Never);
        #endregion
    }

}
