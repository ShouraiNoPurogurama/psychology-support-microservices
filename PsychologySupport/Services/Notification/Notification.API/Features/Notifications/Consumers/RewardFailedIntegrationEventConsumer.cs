using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Features.Notifications.Models;
using Notification.API.Shared.Contracts;

namespace Notification.API.Features.Notifications.Consumers;

public class RewardFailedIntegrationEventConsumer : IConsumer<RewardFailedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<RewardFailedIntegrationEventConsumer> _logger;

    public RewardFailedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        ILogger<RewardFailedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RewardFailedIntegrationEvent> context)
    {
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();
        _logger.LogInformation("Processing RewardFailed {MessageId} reward={RewardId} alias={AliasId}", 
            msgId, msg.RewardId, msg.AliasId);

        // Idempotency
        if (!await _processedEventRepo.TryAddAsync(msgId, nameof(RewardFailedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skip.", msgId);
            return;
        }

        var recipientAliasId = msg.AliasId;

        // Check user preferences
        var prefs = await _preferencesCache.GetOrDefaultAsync(new List<Guid> { recipientAliasId }, context.CancellationToken);
        var pref = prefs.FirstOrDefault();

        if (pref != null && !pref.IsTypeEnabled(NotificationType.Reward))
        {
            _logger.LogInformation("User {AliasId} has disabled Reward notifications, skip.", recipientAliasId);
            return;
        }

        var groupingKey = $"reward_failed:{msg.RewardId}";

        var source = new NotificationSource
        {
            RewardId = msg.RewardId,
            SessionId = msg.SessionId,
            Snippet = $"Không thể xử lý phần thưởng của bạn. {msg.PointsToRefund} điểm đã được hoàn lại."
        };

        // Create notification
        var notification = UserNotification.Create(
            recipientAliasId: recipientAliasId,
            actorAliasId: null, // System notification, no specific actor
            actorDisplayName: "System",
            type: NotificationType.Reward,
            source: source,
            groupingKey: groupingKey
        );

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        // Publish specific RewardFailedNotificationCreatedEvent for real-time delivery
        var rewardFailedNotificationEvent = new RewardFailedNotificationCreatedEvent(
            NotificationId: notification.Id,
            RecipientAliasId: notification.RecipientAliasId,
            RewardId: msg.RewardId,
            SessionId: msg.SessionId,
            ErrorMessage: msg.ErrorMessage,
            PointsRefunded: msg.PointsToRefund,
            Snippet: notification.Snippet ?? string.Empty,
            CreatedAt: notification.CreatedAt
        );

        await context.Publish(rewardFailedNotificationEvent, context.CancellationToken);
        _logger.LogInformation("Created & published reward failure notification {Id} for {Recipient}", 
            notification.Id, recipientAliasId);
    }
}
