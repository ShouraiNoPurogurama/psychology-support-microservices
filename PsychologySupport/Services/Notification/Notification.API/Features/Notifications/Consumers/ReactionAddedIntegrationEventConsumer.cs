using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Features.Notifications.Consumers;

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
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation("Processing ReactionAdded {MessageId} -> {TargetType}:{TargetId}",
            msgId, msg.TargetType, msg.TargetId);

        // Idempotent
        if (!await _processedEventRepo.TryAddAsync(msgId, nameof(ReactionAddedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skip.", msgId);
            return;
        }

        // Preferences
        var prefs = await _preferencesCache.GetOrDefaultAsync(msg.TargetAuthorAliasId, context.CancellationToken);
        if (!prefs.IsTypeEnabled(NotificationType.Reaction))
        {
            _logger.LogDebug("Recipient {Recipient} disabled Reaction notifications.", msg.TargetAuthorAliasId);
            return;
        }

        // Self-protection (thường upstream đã check, nhưng bảo hiểm)
        if (msg.TargetAuthorAliasId == msg.ReactorAliasId)
        {
            _logger.LogDebug("Skip self reaction for alias {AliasId}", msg.ReactorAliasId);
            return;
        }

        // Build source/target
        var source = new NotificationSource
        {
            ReactionId = msg.ReactionId,
            Snippet = $"Đã bày tỏ cảm xúc về" 
        };

        if (msg.TargetType.Equals("post", StringComparison.OrdinalIgnoreCase))
        {
            source.PostId = msg.TargetId;
            source.Snippet += " bài viết của bạn.";
        }
        else if (msg.TargetType.Equals("comment", StringComparison.OrdinalIgnoreCase))
        {
            source.CommentId = msg.TargetId;
            source.Snippet += $" bình luận của bạn: {msg.CommentSnippet}.";
        }

        //Grouping key để tránh flood
        var groupingKey = $"reaction:{msg.TargetType}:{msg.TargetId}";

        //merge nếu trong cửa sổ 30s đã có 1 notif cùng groupingKey
        var merged = await _notificationRepo.TryMergeLatestAsync(
            recipientAliasId: msg.TargetAuthorAliasId,
            groupingKey: groupingKey,
            updater: n =>
            {
                n.ActorAliasId = msg.ReactorAliasId;
                n.ActorDisplayName = msg.ReactorLabel;
                n.Snippet = source.Snippet;
                n.ReactionId = msg.ReactionId;
                n.LastModified = msg.ReactedAt;       
                n.CreatedAt = n.CreatedAt;        
            },
            window: TimeSpan.FromSeconds(30),
            ct: context.CancellationToken
        );

        if (!merged)
        {
            // Tạo mới
            var notification = UserNotification.Create(
                recipientAliasId: msg.TargetAuthorAliasId,
                actorAliasId: msg.ReactorAliasId,
                actorDisplayName: msg.ReactorLabel,
                type: NotificationType.Reaction,
                source: source,
                groupingKey: groupingKey
            );

            // **dùng ReactedAt** để thống nhất thời gian giữa services
            notification.CreatedAt = msg.ReactedAt;

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
                msg.TargetAuthorAliasId, groupingKey);
        }
    }
}
