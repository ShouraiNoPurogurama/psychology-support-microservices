using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Abstractions;
using Notification.API.Models.Notifications;

namespace Notification.API.Features.Consumers.Post;

public class ReactionAddedIntegrationEventConsumer : IConsumer<ReactionAddedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<ReactionAddedIntegrationEventConsumer> _logger;

    public ReactionAddedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        ILogger<ReactionAddedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReactionAddedIntegrationEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing ReactionAdded event {MessageId} for post {PostId}",
            messageId, message.PostId);

        // Idempotency check
        if (!await _processedEventRepo.TryAddAsync(messageId, nameof(ReactionAddedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        // Check preferences
        var preferences = await _preferencesCache.GetOrDefaultAsync(message.AuthorAliasId, context.CancellationToken);
        if (!preferences.IsTypeEnabled(NotificationType.Reaction))
        {
            _logger.LogDebug("User {UserId} has reactions disabled", message.AuthorAliasId);
            return;
        }

        // Create notification
        var source = new NotificationSource
        {
            PostId = message.PostId
        };

        var notification = UserNotification.Create(
            recipientAliasId: message.AuthorAliasId,
            actorAliasId: null, // We don't have actor info in this event
            actorDisplayName: "Someone",
            type: NotificationType.Reaction,
            source: source,
            groupingKey: $"reaction:{message.AuthorAliasId}:{message.PostId}"
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        _logger.LogInformation(
            "Created reaction notification {NotificationId} for user {RecipientId}",
            notification.Id, message.AuthorAliasId);
    }
}
