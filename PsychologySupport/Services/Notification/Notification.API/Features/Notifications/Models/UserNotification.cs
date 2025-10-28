namespace Notification.API.Features.Notifications.Models;

public class UserNotification : AuditableEntity<Guid>
{
    public Guid RecipientAliasId { get; set; }
    public Guid? ActorAliasId { get; set; }
    public string ActorDisplayName { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    
    // Source information
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? ReactionId { get; set; }
    public Guid? FollowId { get; set; }
    public Guid? GiftId { get; set; }
    public Guid? RewardId { get; set; }
    public Guid? SessionId { get; set; }
    public string? ModerationAction { get; set; }
    public string? Snippet { get; set; }
    
    // Aggregation and deduplication
    public string? GroupingKey { get; set; }
    public string? DedupeHash { get; set; }
    
    // Metadata
    public DateTimeOffset? ExpiresAt { get; set; }
    public int Version { get; set; }

    public static UserNotification Create(
        Guid recipientAliasId,
        Guid? actorAliasId,
        string actorDisplayName,
        NotificationType type,
        NotificationSource source,
        string? groupingKey = null,
        DateTimeOffset? expiresAt = null)
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            RecipientAliasId = recipientAliasId,
            ActorAliasId = actorAliasId,
            ActorDisplayName = actorDisplayName,
            Type = type,
            IsRead = false,
            PostId = source.PostId,
            CommentId = source.CommentId,
            ReactionId = source.ReactionId,
            FollowId = source.FollowId,
            GiftId = source.GiftId,
            RewardId = source.RewardId,
            SessionId = source.SessionId,
            ModerationAction = source.ModerationAction,
            Snippet = source.Snippet,
            GroupingKey = groupingKey,
            ExpiresAt = expiresAt,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        notification.DedupeHash = NotificationDedupe.CalculateHash(recipientAliasId, type, source);
        
        return notification;
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        Version++;
        LastModified = DateTimeOffset.UtcNow;
    }
}
