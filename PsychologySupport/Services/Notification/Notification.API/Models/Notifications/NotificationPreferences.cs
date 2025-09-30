namespace Notification.API.Models.Notifications;

public class NotificationPreferences : AuditableEntity<Guid>
{
    // Id is the AliasId
    public bool ReactionsEnabled { get; set; } = true;
    public bool CommentsEnabled { get; set; } = true;
    public bool MentionsEnabled { get; set; } = true;
    public bool FollowsEnabled { get; set; } = true;
    public bool ModerationEnabled { get; set; } = true;
    public bool BotEnabled { get; set; } = true;
    public bool SystemEnabled { get; set; } = true;

    public static NotificationPreferences CreateDefault(Guid aliasId)
    {
        return new NotificationPreferences
        {
            Id = aliasId,
            ReactionsEnabled = true,
            CommentsEnabled = true,
            MentionsEnabled = true,
            FollowsEnabled = true,
            ModerationEnabled = true,
            BotEnabled = true,
            SystemEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
    }

    public bool IsTypeEnabled(NotificationType type)
    {
        return type switch
        {
            NotificationType.Reaction => ReactionsEnabled,
            NotificationType.Comment => CommentsEnabled,
            NotificationType.Mention => MentionsEnabled,
            NotificationType.Follow => FollowsEnabled,
            NotificationType.Moderation => ModerationEnabled,
            NotificationType.BotReply => BotEnabled,
            NotificationType.System => SystemEnabled,
            _ => true
        };
    }
}
