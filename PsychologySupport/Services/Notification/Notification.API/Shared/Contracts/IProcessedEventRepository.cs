namespace Notification.API.Shared.Contracts;

public interface IProcessedEventRepository
{
    Task<bool> TryAddAsync(Guid messageId, string eventType, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default);
    
    Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken cancellationToken = default);
}
