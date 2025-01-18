using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Funcify.Services;
using Moq;
using Xunit;

namespace Funcify.Tests.Services;

public class QueueServiceTests
{
    private readonly Mock<QueueClient> _mockQueueClient;
    private readonly QueueService _queueService;

    public QueueServiceTests()
    {
        _mockQueueClient = new Mock<QueueClient>();
        _queueService = new QueueService(_mockQueueClient.Object);
    }

    [Fact]
public async Task AddMessage_WithValidMessage_ShouldSendMessage()
{
    #region Arrange
    string validMessage = "Test message";

    var mockSendReceipt = QueuesModelFactory.SendReceipt("messageId", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "popReceipt", DateTimeOffset.UtcNow);
    var mockResponse = Response.FromValue(mockSendReceipt, Mock.Of<Response>());

    _mockQueueClient.Setup(client => client.SendMessageAsync(validMessage))
                    .ReturnsAsync(mockResponse);
    #endregion

    #region Act
    await _queueService.AddMessage(validMessage);
    #endregion

    #region Assert
    _mockQueueClient.Verify(client => client.SendMessageAsync(validMessage), Times.Once);
    #endregion
}


    [Fact]
    public async Task AddMessage_WithNullMessage_ShouldThrowArgumentException()
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _queueService.AddMessage(null!));
        #endregion
    }

    [Fact]
    public async Task AddMessage_WithEmptyMessage_ShouldThrowArgumentException()
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _queueService.AddMessage(string.Empty));
        #endregion
    }
}
