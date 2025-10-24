using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using RealtimeHub.API.Models;
using RealtimeHub.API.Services;

namespace RealtimeHub.API.Consumers;

/// <summary>
/// Consumes NotificationCreated events from Notification Service
/// and delivers them to connected clients via SignalR
/// </summary>
public class NotificationCreatedIntegrationEventConsumer : IConsumer<NotificationCreatedIntegrationEvent>
{
    private readonly IRealtimeHubService _realtimeHubService;
    private readonly ILogger<NotificationCreatedIntegrationEventConsumer> _logger;

    public NotificationCreatedIntegrationEventConsumer(
        IRealtimeHubService realtimeHubService,
        ILogger<NotificationCreatedIntegrationEventConsumer> logger)
    {
        _realtimeHubService = realtimeHubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received NotificationCreated event for notification {NotificationId}, recipient {RecipientId}",
            message.NotificationId, message.RecipientAliasId);

        // Map to NotificationMessage
        var notificationMessage = new NotificationMessage
        {
            Id = message.NotificationId,
            RecipientAliasId = message.RecipientAliasId,
            ActorAliasId = message.ActorAliasId,
            ActorDisplayName = message.ActorDisplayName,
            Type = message.NotificationType,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            PostId = message.PostId,
            CommentId = message.CommentId,
            ReactionId = message.ReactionId,
            GiftId = message.GiftId,
            FollowId = message.FollowId,
            ModerationAction = message.ModerationAction,
            Snippet = message.Snippet,
            CreatedAt = message.CreatedAt
        };

        // Send to user via SignalR
        await _realtimeHubService.SendNotificationToUserAsync(
            message.RecipientAliasId,
            notificationMessage,
            context.CancellationToken);

        _logger.LogInformation(
            "Sent notification {NotificationId} to user {RecipientId} via SignalR",
            message.NotificationId, message.RecipientAliasId);
    }
}
