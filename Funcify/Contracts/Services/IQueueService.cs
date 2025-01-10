namespace Funcify.Contracts.Services;

public interface IQueueService {
    public Task AddMessage(string message);
}