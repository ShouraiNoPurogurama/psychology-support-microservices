using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Features.Notifications.Models;
using Notification.API.Shared.Contracts;

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
    var msg = context.Message;
    var msgId = context.MessageId ?? Guid.NewGuid();
    _logger.LogInformation("Processing AliasFollowed {MessageId}: {Follower} -> {Followed}", msgId, msg.FollowerAliasId, msg.FollowedAliasId);

    // Idempotency
    if (!await _processedEventRepo.TryAddAsync(msgId, nameof(AliasFollowedIntegrationEvent), context.CancellationToken))
    {
        _logger.LogInformation("Event {MessageId} already processed, skip.", msgId);
        return;
    }

    // Preferences
    var prefs = await _preferencesCache.GetOrDefaultAsync(msg.FollowedAliasId, context.CancellationToken);
    if (!prefs.IsTypeEnabled(NotificationType.Follow))
    {
        _logger.LogDebug("Recipient {Recipient} disabled Follow notifications.", msg.FollowedAliasId);
        return;
    }

    var source = new NotificationSource
    {
        FollowId = msg.FollowerAliasId,
        Snippet = "đã theo dõi bạn"
    };

    var groupingKey = $"follow:{msg.FollowedAliasId}";

    // Merge trong 30s để tránh tạo nhiều notif liên tiếp
    var merged = await _notificationRepo.TryMergeLatestAsync(
        recipientAliasId: msg.FollowedAliasId,
        groupingKey: groupingKey,
        updater: n =>
        {
            n.ActorAliasId = msg.FollowerAliasId;
            n.ActorDisplayName = msg.FollowerAliasLabel;
            n.Snippet = source.Snippet;
            n.FollowId = msg.FollowerAliasId; 
        },
        window: TimeSpan.FromSeconds(30),
        ct: context.CancellationToken
    );

    if (!merged)
    {
        var notification = UserNotification.Create(
            recipientAliasId: msg.FollowedAliasId,
            actorAliasId: msg.FollowerAliasId,
            actorDisplayName: msg.FollowerAliasLabel,
            type: NotificationType.Follow,
            source: source,
            groupingKey: groupingKey
        );

        notification.CreatedAt = msg.FollowedAt;

        await _notificationRepo.AddAsync(notification, context.CancellationToken);

        var createdEvt = new NotificationCreatedIntegrationEvent(
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
            GiftId: notification.GiftId,
            FollowId: notification.FollowId,
                    RewardId: notification.RewardId,
                    SessionId: notification.SessionId,
 ModerationAction: notification.ModerationAction,
            Snippet: notification.Snippet,
            CreatedAt: notification.CreatedAt
        );

        await context.Publish(createdEvt, context.CancellationToken);
        _logger.LogInformation("Created & published follow notification {Id} for {Recipient}", notification.Id, msg.FollowedAliasId);
    }
    else
    {
        _logger.LogInformation("Merged follow notification for {Recipient} G={Grouping}", msg.FollowedAliasId, groupingKey);
    }
}

}
