using Notification.API.Features.Notifications.Models;

namespace Notification.API.Contracts;

public interface INotificationRepository
{
    Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<(List<UserNotification> Items, bool HasMore)> GetPagedAsync(
        Guid recipientAliasId,
        int limit,
        DateTimeOffset? cursorCreatedAt = null,
        Guid? cursorId = null,
        NotificationType? type = null,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);
    
    Task<int> GetUnreadCountAsync(Guid recipientAliasId, CancellationToken cancellationToken = default);
    
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default);
    
    Task AddManyAsync(IEnumerable<UserNotification> notifications, CancellationToken cancellationToken = default);
    
    Task<int> MarkReadAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default);
    
    Task<int> MarkAllReadBeforeAsync(Guid recipientAliasId, DateTimeOffset cutoffTime, CancellationToken cancellationToken = default);
    
    Task<int> DeleteBySourceAsync(Guid? postId = null, Guid? commentId = null, CancellationToken cancellationToken = default);
    
    Task<bool> TryMergeLatestAsync(Guid recipientAliasId, string groupingKey, Action<UserNotification> updater, TimeSpan window, CancellationToken ct);

}
