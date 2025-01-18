using Funcify.Actions;
using Funcify.Contracts.Services;
using Moq;
using Xunit;

namespace Funcify.Tests.Actions;

public class EnqueueTaskTests
{
    private readonly Mock<IQueueService> _mockQueueService;
    private readonly EnqueueTask _enqueueTask;

    public EnqueueTaskTests()
    {
        _mockQueueService = new Mock<IQueueService>();
        _enqueueTask = new EnqueueTask(_mockQueueService.Object);
    }

    [Fact]
    public async Task Invoke_WithValidMessage_ShouldEnqueueTask()
    {
        #region Arrange
        string validMessage = "Process image task";
        _mockQueueService.Setup(service => service.AddMessage(validMessage))
                         .Returns(Task.CompletedTask);
        #endregion

        #region Act
        await _enqueueTask.Invoke(validMessage);
        #endregion

        #region Assert
        _mockQueueService.Verify(service => service.AddMessage(validMessage), Times.Once);
        #endregion
    }

    [Fact]
    public async Task Invoke_WithNullMessage_ShouldThrowArgumentException()
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _enqueueTask.Invoke(null!));
        #endregion
    }

    [Fact]
    public async Task Invoke_WithEmptyMessage_ShouldThrowArgumentException()
    {
        #region Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _enqueueTask.Invoke(string.Empty));
        #endregion
    }
}
