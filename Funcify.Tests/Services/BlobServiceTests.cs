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
    public async Task GetContainer_IfNotExists_WithValidContainerName_ShouldGetContainer()
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
        await _blobService.GetContainer(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Once);
        #endregion
    }

    [Fact]
    public async Task GetContainer_WithExistingContainer_ShouldNotGetContainer()
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
        await _blobService.GetContainer(containerName);
        #endregion

        #region Assert
        mockBlobContainerClient.Verify(client => client.ExistsAsync(default), Times.Once);

        mockBlobContainerClient.Verify(client => client.CreateIfNotExistsAsync(default, null, null, default), Times.Never);
        #endregion
    }

    [Fact]
    public async Task GetContainer_WhenBlobServiceClientThrows_ShouldPropagateException()
    {
        #region Arrange
        _mockBlobServiceClient
                .Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
                .Throws(It.IsAny<Exception>);
        #endregion

        #region Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () => await _blobService.GetContainer("unprocessed-images"));

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

        var blobUri = new Uri($"https://mockstorageaccount.blob.core.windows.net/{containerName}/{blobName}");

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();

        _mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(containerName))
                          .Returns(mockBlobContainerClient.Object);

        mockBlobContainerClient.Setup(container => container.GetBlobClient(blobName))
                               .Returns(mockBlobClient.Object);

        mockBlobContainerClient.Setup(client => client.ExistsAsync(default))
                                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        mockBlobClient.Setup(client => client.Uri)
            .Returns(blobUri);

        mockBlobClient.Setup(blob => blob.UploadAsync(It.IsAny<Stream>(), true, default))
                      .ReturnsAsync(Response.FromValue(Mock.Of<BlobContentInfo>(), Mock.Of<Response>()));
        #endregion

        #region Act
        var result = await _blobService.UploadBlob(containerName, blobName, data);
        #endregion

        #region Assert
        Assert.Equal(blobUri.ToString(), result);
        mockBlobContainerClient.Verify(container => container.GetBlobClient(blobName), Times.Once);
        mockBlobClient.Verify(blob => blob.UploadAsync(data, true, default), Times.Once);
        #endregion
    }

    [Fact]
    public async Task DownloadBlobToFile_WithValidInputs_ReturnsFilePath()
    {
        #region Arrange
        string containerName = "unprocessed-images";
        string blobName = "valid-image.jpg";

        string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), containerName, blobName);

        Mock<BlobContainerClient> _mockBlobContainerClient = new Mock<BlobContainerClient>();
        Mock<BlobClient> _mockBlobClient = new Mock<BlobClient>();

        _mockBlobServiceClient
            .Setup(client => client.GetBlobContainerClient(containerName))
            .Returns(_mockBlobContainerClient.Object);

        _mockBlobContainerClient
            .Setup(client => client.GetBlobClient(blobName))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        _mockBlobClient.Setup(client => client.DownloadToAsync(downloadPath, default))
            .ReturnsAsync(Response.FromValue(Mock.Of<Response>(), Mock.Of<Response>()));
        #endregion

        #region Act
        string result = await _blobService.DownloadBlob(containerName, blobName);
        #endregion

        #region Assert
        Assert.Equal(downloadPath, result);

        _mockBlobServiceClient.Verify(client => client.GetBlobContainerClient(containerName), Times.Once);
        _mockBlobContainerClient.Verify(client => client.GetBlobClient(blobName), Times.Once);
        _mockBlobClient.Verify(client => client.DownloadToAsync(downloadPath, default), Times.Once);
        #endregion
    }

    [Theory]
    [InlineData("", "valid-image.jpg")]
    [InlineData(null, "valid-image.jpg")]
    [InlineData("unprocessed-images", null)]
    [InlineData("unprocessed-images", "")]
    public async Task DownloadBlobToFile_WithInvalidContainer_ThrowsException(string containerName, string blobName)
    {
        #region Arrange
        _mockBlobServiceClient
            .Setup(client => client.GetBlobContainerClient(containerName))
            .Throws(new ArgumentException());
        #endregion

        #region Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _blobService.DownloadBlob(containerName, blobName));
        #endregion
    }

    [Fact]
public async Task DownloadBlobToFile_WithNonExistentBlob_ThrowsException() {
    #region Arrange
        string containerName = "unprocessed-images";
        string blobName = "nonexistent-image.jpg";

        string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), containerName, blobName);

        Mock<BlobContainerClient> _mockBlobContainerClient = new Mock<BlobContainerClient>();
        Mock<BlobClient> _mockBlobClient = new Mock<BlobClient>();

        _mockBlobServiceClient
            .Setup(client => client.GetBlobContainerClient(containerName))
            .Returns(_mockBlobContainerClient.Object);

        _mockBlobContainerClient
            .Setup(client => client.GetBlobClient(blobName))
            .Returns(_mockBlobClient.Object);

        _mockBlobClient.Setup(client => client.ExistsAsync(default))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));
    #endregion

    #region Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => _blobService.DownloadBlob(containerName, blobName));
    #endregion
}


}
