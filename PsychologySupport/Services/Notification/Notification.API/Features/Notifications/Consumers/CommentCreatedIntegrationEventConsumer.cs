using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;
using Notification.API.Hubs;

namespace Notification.API.Features.Notifications.Consumers;

public class CommentCreatedIntegrationEventConsumer : IConsumer<CommentCreatedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<CommentCreatedIntegrationEventConsumer> _logger;

    public CommentCreatedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        INotificationHubService hubService,
        ILogger<CommentCreatedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _hubService = hubService;
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

        // Send real-time notification via SignalR
        await _hubService.SendNotificationToUserAsync(recipientAliasId, notification, context.CancellationToken);
    }
}
