namespace Notification.API.Features.Notifications.Models;

public enum NotificationType
{
    Reaction = 1,
    Comment = 2,
    Mention = 3,
    Follow = 4,
    Moderation = 5,
    BotReply = 6,
    System = 7,
    Gift = 8
}
