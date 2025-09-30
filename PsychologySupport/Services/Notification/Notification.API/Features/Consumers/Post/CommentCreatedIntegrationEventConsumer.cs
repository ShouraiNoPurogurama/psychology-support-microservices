using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Abstractions;
using Notification.API.Models.Notifications;

namespace Notification.API.Features.Consumers.Post;

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

        // Check preferences
        var preferences = await _preferencesCache.GetOrDefaultAsync(message.AuthorAliasId, context.CancellationToken);
        if (!preferences.IsTypeEnabled(NotificationType.Comment))
        {
            _logger.LogDebug("User {UserId} has comments disabled", message.AuthorAliasId);
            return;
        }

        // Create notification
        var source = new NotificationSource
        {
            PostId = message.PostId
        };

        var notification = UserNotification.Create(
            recipientAliasId: message.AuthorAliasId,
            actorAliasId: null,
            actorDisplayName: "Someone",
            type: NotificationType.Comment,
            source: source,
            groupingKey: $"comment:{message.AuthorAliasId}:{message.PostId}"
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        _logger.LogInformation(
            "Created comment notification {NotificationId} for user {RecipientId}",
            notification.Id, message.AuthorAliasId);
    }
}
