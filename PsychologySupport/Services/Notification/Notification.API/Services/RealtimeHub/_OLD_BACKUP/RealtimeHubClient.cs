using Notification.API.Features.Notifications.Models;
using System.Net.Http.Json;

namespace Notification.API.Services.RealtimeHub;

/// <summary>
/// HTTP client for sending real-time notifications to RealtimeHub service
/// </summary>
public class RealtimeHubClient : IRealtimeHubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RealtimeHubClient> _logger;

    public RealtimeHubClient(HttpClient httpClient, ILogger<RealtimeHubClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
        Guid aliasId,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                aliasId,
                notification = MapToNotificationMessage(notification)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/realtime/send-to-user",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Sent real-time notification {NotificationId} to user {AliasId} via RealtimeHub",
                    notification.Id, aliasId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send real-time notification {NotificationId} to user {AliasId}. Status: {StatusCode}",
                    notification.Id, aliasId, response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            // Don't throw - real-time delivery is best effort
            // The notification is already saved in database
            _logger.LogWarning(ex,
                "HTTP error sending real-time notification {NotificationId} to user {AliasId}. " +
                "Notification is saved in database and will be available on refresh.",
                notification.Id, aliasId);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex,
                "Timeout sending real-time notification {NotificationId} to user {AliasId}. " +
                "Notification is saved in database.",
                notification.Id, aliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error sending real-time notification {NotificationId} to user {AliasId}",
                notification.Id, aliasId);
        }
    }

    public async Task SendNotificationToUsersAsync(
        List<Guid> aliasIds,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                aliasIds,
                notification = MapToNotificationMessage(notification)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/realtime/send-to-users",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Sent real-time notification {NotificationId} to {UserCount} users via RealtimeHub",
                    notification.Id, aliasIds.Count);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send real-time notification {NotificationId} to {UserCount} users. Status: {StatusCode}",
                    notification.Id, aliasIds.Count, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Error sending real-time notification {NotificationId} to {UserCount} users. " +
                "Notification is saved in database.",
                notification.Id, aliasIds.Count);
        }
    }

    private object MapToNotificationMessage(UserNotification notification)
    {
        return new
        {
            id = notification.Id,
            recipientAliasId = notification.RecipientAliasId,
            actorAliasId = notification.ActorAliasId,
            actorDisplayName = notification.ActorDisplayName,
            type = notification.Type.ToString(),
            isRead = notification.IsRead,
            readAt = notification.ReadAt,
            postId = notification.PostId,
            commentId = notification.CommentId,
            reactionId = notification.ReactionId,
            followId = notification.FollowId,
            moderationAction = notification.ModerationAction,
            snippet = notification.Snippet,
            createdAt = notification.CreatedAt
        };
    }
}
