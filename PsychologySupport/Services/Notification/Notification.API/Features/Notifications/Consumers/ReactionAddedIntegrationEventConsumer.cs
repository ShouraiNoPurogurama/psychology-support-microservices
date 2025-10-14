using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;
using Notification.API.Hubs;

namespace Notification.API.Features.Notifications.Consumers;

public class ReactionAddedIntegrationEventConsumer : IConsumer<ReactionAddedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<ReactionAddedIntegrationEventConsumer> _logger;

    public ReactionAddedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        INotificationHubService hubService,
        ILogger<ReactionAddedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReactionAddedIntegrationEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing ReactionAdded event {MessageId} for {TargetType} {TargetId}",
            messageId, message.TargetType, message.TargetId);

        // Idempotency check
        if (!await _processedEventRepo.TryAddAsync(messageId, nameof(ReactionAddedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        // Check preferences
        var preferences = await _preferencesCache.GetOrDefaultAsync(message.TargetAuthorAliasId, context.CancellationToken);
        if (!preferences.IsTypeEnabled(NotificationType.Reaction))
        {
            _logger.LogDebug("User {UserId} has reactions disabled", message.TargetAuthorAliasId);
            return;
        }

        // Create notification
        var source = new NotificationSource
        {
            ReactionId = message.ReactionId,
            Snippet = $"reacted with {message.ReactionCode}"
        };

        // Set PostId or CommentId based on target type
        if (message.TargetType.Equals("post", StringComparison.OrdinalIgnoreCase))
        {
            source.PostId = message.TargetId;
        }
        else if (message.TargetType.Equals("comment", StringComparison.OrdinalIgnoreCase))
        {
            source.CommentId = message.TargetId;
        }

        var notification = UserNotification.Create(
            recipientAliasId: message.TargetAuthorAliasId,
            actorAliasId: message.ReactorAliasId,
            actorDisplayName: message.ReactorLabel,
            type: NotificationType.Reaction,
            source: source,
            groupingKey: $"reaction:{message.TargetType}:{message.TargetId}"
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        _logger.LogInformation(
            "Created reaction notification {NotificationId} for user {RecipientId}",
            notification.Id, message.TargetAuthorAliasId);

        // Send real-time notification via SignalR
        await _hubService.SendNotificationToUserAsync(message.TargetAuthorAliasId, notification, context.CancellationToken);
    }
}
