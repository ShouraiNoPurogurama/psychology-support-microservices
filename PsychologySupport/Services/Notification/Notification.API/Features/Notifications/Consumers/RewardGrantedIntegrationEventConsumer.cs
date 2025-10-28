using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Features.Notifications.Models;
using Notification.API.Shared.Contracts;

namespace Notification.API.Features.Notifications.Consumers;

public class RewardGrantedIntegrationEventConsumer : IConsumer<RewardGrantedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<RewardGrantedIntegrationEventConsumer> _logger;

    public RewardGrantedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        IPreferencesCache preferencesCache,
        ILogger<RewardGrantedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RewardGrantedIntegrationEvent> context)
    {
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();
        _logger.LogInformation("Processing RewardGranted {MessageId} reward={RewardId} alias={AliasId}", 
            msgId, msg.RewardId, msg.AliasId);

        // Idempotency
        if (!await _processedEventRepo.TryAddAsync(msgId, nameof(RewardGrantedIntegrationEvent), context.CancellationToken))
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

        var groupingKey = $"reward:{msg.RewardId}";

        var source = new NotificationSource
        {
            RewardId = msg.RewardId,
            SessionId = msg.SessionId,
            Snippet = $"Phần thưởng của bạn đã sẵn sàng, hãy vào Emo chat để nhận sticker."
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

        // Publish specific RewardNotificationCreatedEvent for real-time delivery
        var rewardNotificationEvent = new RewardNotificationCreatedEvent(
            NotificationId: notification.Id,
            RecipientAliasId: notification.RecipientAliasId,
            RewardId: msg.RewardId,
            SessionId: msg.SessionId,
            StickerUrl: msg.StickerUrl,
            PromptFilter: msg.PromptFilter,
            Snippet: notification.Snippet ?? string.Empty,
            CreatedAt: notification.CreatedAt
        );

        await context.Publish(rewardNotificationEvent, context.CancellationToken);
        _logger.LogInformation("Created & published reward notification {Id} for {Recipient}", 
            notification.Id, recipientAliasId);
    }
}
