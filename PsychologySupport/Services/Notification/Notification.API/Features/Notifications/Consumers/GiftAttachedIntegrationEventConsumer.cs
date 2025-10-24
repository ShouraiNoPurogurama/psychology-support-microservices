using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Notification.API.Features.Notifications.Models;
using Notification.API.Shared.Contracts;

namespace Notification.API.Features.Notifications.Consumers;

public class GiftAttachedIntegrationEventConsumer : IConsumer<GiftAttachedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly IPreferencesCache _preferencesCache;
    private readonly ILogger<GiftAttachedIntegrationEventConsumer> _logger;

    public GiftAttachedIntegrationEventConsumer(INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo, IPreferencesCache preferencesCache,
        ILogger<GiftAttachedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _preferencesCache = preferencesCache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GiftAttachedIntegrationEvent> context)
    {
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation("Processing Gift Attached {MessageId} -> {TargetId}",
            msgId, msg.PostId);

        // Idempotent
        if (!await _processedEventRepo.TryAddAsync(msgId, nameof(GiftAttachedIntegrationEventConsumer), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skip.", msgId);
            return;
        }

        // Preferences
        var prefs = await _preferencesCache.GetOrDefaultAsync(msg.PostAuthorAliasId, context.CancellationToken);
        if (!prefs.IsTypeEnabled(NotificationType.Reaction))
        {
            _logger.LogDebug("Recipient {Recipient} disabled Gift notifications.", msg.PostAuthorAliasId);
            return;
        }

        // Self-protection (thường upstream đã check, nhưng bảo hiểm)
        if (msg.PostAuthorAliasId == msg.SenderAliasId)
        {
            _logger.LogDebug("Skip self gift sent for alias {AliasId}", msg.SenderAliasId);
            return;
        }

        // Build source/target
        var source = new NotificationSource
        {
            GiftId = msg.GiftId,
            Snippet = $"Đã gửi bạn một món quà với thông điệp: {msg.Message}"
        };
        
        //Grouping key để tránh flood
        var groupingKey = $"gift:post:{msg.PostId}";

        //merge nếu trong cửa sổ 30s đã có 1 notif cùng groupingKey
        var merged = await _notificationRepo.TryMergeLatestAsync(
            recipientAliasId: msg.PostAuthorAliasId,
            groupingKey: groupingKey,
            updater: n =>
            {
                n.ActorAliasId = msg.SenderAliasId;
                n.ActorDisplayName = msg.SenderAliasLabel;
                n.Snippet = source.Snippet;
                n.GiftId = msg.GiftId;
                n.LastModified = msg.SentAt;
                n.CreatedAt = n.CreatedAt;
            },
            window: TimeSpan.FromSeconds(30),
            ct: context.CancellationToken
        );

        if (!merged)
        {
            // Tạo mới
            var notification = UserNotification.Create(
                recipientAliasId: msg.PostAuthorAliasId,
                actorAliasId: msg.SenderAliasId,
                actorDisplayName: msg.SenderAliasLabel,
                type: NotificationType.Gift,
                source: source,
                groupingKey: groupingKey
            );

            // **dùng SentAt** để thống nhất thời gian giữa services
            notification.CreatedAt = msg.SentAt;

            await _notificationRepo.AddAsync(notification, context.CancellationToken);

            // Publish IntegrationEvent
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
                ModerationAction: notification.ModerationAction,
                Snippet: notification.Snippet,
                CreatedAt: notification.CreatedAt
            );

            await context.Publish(createdEvt, context.CancellationToken);

            _logger.LogInformation("Created & published notification {Id} for {Recipient}",
                notification.Id, notification.RecipientAliasId);
        }
        else
        {
            _logger.LogInformation("Merged reaction notification for recipient {Recipient} G={Grouping}",
                msg.PostAuthorAliasId, groupingKey);
        }
    }
}