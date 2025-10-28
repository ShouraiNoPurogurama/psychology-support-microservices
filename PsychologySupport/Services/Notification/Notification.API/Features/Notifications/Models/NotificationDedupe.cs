using System.Security.Cryptography;
using System.Text;

namespace Notification.API.Features.Notifications.Models;

public static class NotificationDedupe
{
    public static string CalculateHash(Guid recipientAliasId, NotificationType type, NotificationSource source)
    {
        var sourceId = source.PostId ?? source.CommentId ?? source.ReactionId ?? source.FollowId ?? source.GiftId ?? Guid.Empty;
        var input = $"{recipientAliasId}|{(int)type}|{sourceId}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
