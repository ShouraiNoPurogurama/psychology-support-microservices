using Microsoft.EntityFrameworkCore;
using Notification.API.Contracts;
using Notification.API.Data;
using Notification.API.Features.Notifications.Models;
using Notification.API.Infrastructure.Data;

namespace Notification.API.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<(List<UserNotification> Items, bool HasMore)> GetPagedAsync(
        Guid recipientAliasId,
        int limit,
        DateTimeOffset? cursorCreatedAt = null,
        Guid? cursorId = null,
        NotificationType? type = null,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserNotifications
            .Where(n => n.RecipientAliasId == recipientAliasId);

        // Apply filters
        if (type.HasValue)
        {
            query = query.Where(n => n.Type == type.Value);
        }

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        // Apply cursor pagination (keyset)
        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            query = query.Where(n =>
                n.CreatedAt < cursorCreatedAt.Value ||
                (n.CreatedAt == cursorCreatedAt.Value && n.Id < cursorId.Value));
        }

        // Fetch limit + 1 to check if there are more items
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
        {
            items = items.Take(limit).ToList();
        }

        return (items, hasMore);
    }

    public async Task<int> GetUnreadCountAsync(Guid recipientAliasId, CancellationToken cancellationToken = default)
    {
        return await _context.UserNotifications
            .Where(n => n.RecipientAliasId == recipientAliasId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        await _context.UserNotifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddManyAsync(IEnumerable<UserNotification> notifications, CancellationToken cancellationToken = default)
    {
        await _context.UserNotifications.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> MarkReadAsync(IEnumerable<Guid> notificationIds, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.UserNotifications
            .Where(n => notificationIds.Contains(n.Id) && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.LastModified, now),
                cancellationToken);
    }

    public async Task<int> MarkAllReadBeforeAsync(Guid recipientAliasId, DateTimeOffset cutoffTime, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.UserNotifications
            .Where(n => n.RecipientAliasId == recipientAliasId &&
                       !n.IsRead &&
                       n.CreatedAt < cutoffTime)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.LastModified, now),
                cancellationToken);
    }

    public async Task<int> DeleteBySourceAsync(Guid? postId = null, Guid? commentId = null, CancellationToken cancellationToken = default)
    {
        if (!postId.HasValue && !commentId.HasValue)
        {
            return 0;
        }

        var query = _context.UserNotifications.AsQueryable();

        if (postId.HasValue)
        {
            query = query.Where(n => n.PostId == postId.Value);
        }

        if (commentId.HasValue)
        {
            query = query.Where(n => n.CommentId == commentId.Value);
        }

        return await query.ExecuteDeleteAsync(cancellationToken);
    }
}
