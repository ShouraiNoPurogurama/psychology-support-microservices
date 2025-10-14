using Microsoft.AspNetCore.SignalR;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Hubs;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationHubService> _logger;

    public NotificationHubService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
        Guid aliasId,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationDto = new
            {
                notification.Id,
                notification.RecipientAliasId,
                notification.ActorAliasId,
                notification.ActorDisplayName,
                Type = notification.Type.ToString(),
                notification.IsRead,
                notification.ReadAt,
                notification.PostId,
                notification.CommentId,
                notification.ReactionId,
                notification.FollowId,
                notification.ModerationAction,
                notification.Snippet,
                notification.CreatedAt
            };

            await _hubContext.Clients
                .Group($"user_{aliasId}")
                .SendAsync("ReceiveNotification", notificationDto, cancellationToken);

            _logger.LogInformation(
                "Sent notification {NotificationId} to user {AliasId} via SignalR",
                notification.Id, aliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send notification {NotificationId} to user {AliasId} via SignalR",
                notification.Id, aliasId);
        }
    }

    public async Task SendNotificationToUsersAsync(
        List<Guid> aliasIds,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        foreach (var aliasId in aliasIds)
        {
            await SendNotificationToUserAsync(aliasId, notification, cancellationToken);
        }
    }
}
