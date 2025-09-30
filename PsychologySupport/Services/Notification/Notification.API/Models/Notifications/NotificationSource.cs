namespace Notification.API.Models.Notifications;

public class NotificationSource
{
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? ReactionId { get; set; }
    public Guid? FollowId { get; set; }
    public string? ModerationAction { get; set; }
    public string? Snippet { get; set; }
}
