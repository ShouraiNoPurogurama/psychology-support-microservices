using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Features.Notifications.Consumers;

public class CommentCreatedIntegrationEventConsumer : IConsumer<CommentCreatedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<CommentCreatedIntegrationEventConsumer> _logger;

    public CommentCreatedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        ILogger<CommentCreatedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommentCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing CommentCreated event {MessageId} for post {PostId}",
            messageId, message.PostId);

        // Idempotency check
        if (!await _processedEventRepo.TryAddAsync(messageId, nameof(CommentCreatedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        // Determine recipient: parent comment author if reply, otherwise post author
        Guid recipientAliasId;
        string groupingKeySuffix;
        
        if (message.ParentCommentId.HasValue && message.ParentCommentAuthorAliasId.HasValue)
        {
            recipientAliasId = message.ParentCommentAuthorAliasId.Value;
            groupingKeySuffix = $"comment-reply:{message.ParentCommentId.Value}";
        }
        else
        {
            recipientAliasId = message.PostAuthorAliasId;
            groupingKeySuffix = $"comment:{message.PostId}";
        }

        // Don't notify if commenting on own content (already checked in event handler, but double-check)
        if (recipientAliasId == message.CommentAuthorAliasId)
        {
            _logger.LogDebug("User commented on their own content, skipping notification");
            return;
        }

        // Check preferences
        var preferences = await _preferencesCache.GetOrDefaultAsync(recipientAliasId, context.CancellationToken);
        if (!preferences.IsTypeEnabled(NotificationType.Comment))
        {
            _logger.LogDebug("User {UserId} has comments disabled", recipientAliasId);
            return;
        }

        // Create notification
        var source = new NotificationSource
        {
            PostId = message.PostId,
            CommentId = message.CommentId,
            Snippet = message.CommentSnippet
        };

        var notification = UserNotification.Create(
            recipientAliasId: recipientAliasId,
            actorAliasId: message.CommentAuthorAliasId,
            actorDisplayName: message.CommentAuthorLabel,
            type: NotificationType.Comment,
            source: source,
            groupingKey: groupingKeySuffix
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        _logger.LogInformation(
            "Created comment notification {NotificationId} for user {RecipientId}",
            notification.Id, recipientAliasId);

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
