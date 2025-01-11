using System;
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
    public async Task CreateContainerIfNotExists_WithValidContainerName_ShouldCreateContainer()
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
        await _blobService.CreateContainerIfNotExistsAsync(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Once);
        #endregion
    }

    [Fact]
    public async Task CreateContainerIfNotExists_WithExistingContainer_ShouldNotCreateContainer()
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
        await _blobService.CreateContainerIfNotExistsAsync(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Never);
        #endregion
    }

    [Theory]
    [InlineData([""])]
    [InlineData([null])]
    public async Task CreateContainerIfNotExists_WithInvalidContainerName_ShouldThrowArgumentException(string containerName)
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _blobService.CreateContainerIfNotExistsAsync(containerName));
        #endregion
    }

    [Fact]
    public async Task CreateContainerIfNotExists_WhenBlobServiceClientThrows_ShouldPropagateException()
    {
        #region Arrange
        _mockBlobServiceClient
                .Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Throws(It.IsAny<Exception>);
        #endregion

        #region Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () => await _blobService.CreateContainerIfNotExistsAsync("unprocessed-images"));

        _mockBlobServiceClient.Verify(client => client.GetBlobContainerClient("unprocessed-images"), Times.Once);
        #endregion
    }
}
