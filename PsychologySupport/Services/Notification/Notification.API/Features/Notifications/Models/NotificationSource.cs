namespace Notification.API.Features.Notifications.Models;

public class NotificationSource
{
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? ReactionId { get; set; }
    public Guid? FollowId { get; set; }
    public Guid? GiftId { get; set; }
    public Guid? RewardId { get; set; }
    public Guid? SessionId { get; set; }
    public string? ModerationAction { get; set; }
    public string? Snippet { get; set; }
}
