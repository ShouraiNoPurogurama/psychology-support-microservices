using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Features.Notifications.Consumers;

public class AliasFollowedIntegrationEventConsumer : IConsumer<AliasFollowedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<AliasFollowedIntegrationEventConsumer> _logger;

    public AliasFollowedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        ILogger<AliasFollowedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AliasFollowedIntegrationEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing AliasFollowed event {MessageId}: {Follower} followed {Followed}",
            messageId, message.FollowerAliasId, message.FollowedAliasId);

        // Idempotency check
        if (!await _processedEventRepo.TryAddAsync(messageId, nameof(AliasFollowedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        // Check preferences
        var preferences = await _preferencesCache.GetOrDefaultAsync(message.FollowedAliasId, context.CancellationToken);
        if (!preferences.IsTypeEnabled(NotificationType.Follow))
        {
            _logger.LogDebug("User {UserId} has follows disabled", message.FollowedAliasId);
            return;
        }

        // Create notification
        var source = new NotificationSource
        {
            FollowId = message.FollowerAliasId
        };

        var notification = UserNotification.Create(
            recipientAliasId: message.FollowedAliasId,
            actorAliasId: message.FollowerAliasId,
            actorDisplayName: "Someone",
            type: NotificationType.Follow,
            source: source
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        _logger.LogInformation(
            "Created follow notification {NotificationId} for user {RecipientId}",
            notification.Id, message.FollowedAliasId);

        // Publish NotificationCreated event for delivery services (RealtimeHub, Email, Firebase)
        var notificationCreatedEvent = new NotificationCreatedIntegrationEvent(
            NotificationId: notification.Id,
            RecipientAliasId: notification.RecipientAliasId,
            ActorAliasId: notification.ActorAliasId,
            ActorDisplayName: notification.ActorDisplayName,
            NotificationType: notification.Type.ToString(),
            IsRead: notification.IsRead,
            ReadAt: notification.ReadAt,
            PostId: notification.PostId,
            CommentId: notification.CommentId,
            ReactionId: notification.ReactionId,
            FollowId: notification.FollowId,
            ModerationAction: notification.ModerationAction,
            Snippet: notification.Snippet,
            CreatedAt: notification.CreatedAt
        );

        await context.Publish(notificationCreatedEvent, context.CancellationToken);

        _logger.LogInformation(
            "Published NotificationCreated event for notification {NotificationId}",
            notification.Id);
    }
}
