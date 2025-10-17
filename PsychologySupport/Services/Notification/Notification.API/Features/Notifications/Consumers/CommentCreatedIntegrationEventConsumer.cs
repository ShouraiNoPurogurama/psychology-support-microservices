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
        var msg = context.Message;
        var msgId = context.MessageId ?? Guid.NewGuid();
        _logger.LogInformation("Processing CommentCreated {MessageId} post={PostId} cmt={CommentId}", msgId, msg.PostId,
            msg.CommentId);

        // Idempotency
        if (!await _processedEventRepo.TryAddAsync(msgId, nameof(CommentCreatedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skip.", msgId);
            return;
        }

        // Recipient + grouping
        Guid recipientAliasId;
        string groupingKey;
        if (msg.ParentCommentId.HasValue && msg.ParentCommentAuthorAliasId.HasValue)
        {
            recipientAliasId = msg.ParentCommentAuthorAliasId.Value;
            groupingKey = $"comment-reply:{msg.ParentCommentId.Value}";
        }
        else
        {
            recipientAliasId = msg.PostAuthorAliasId;
            groupingKey = $"comment:{msg.PostId}";
        }

        if (recipientAliasId == msg.CommentAuthorAliasId)
        {
            _logger.LogDebug("Skip self comment notification for alias {Alias}", recipientAliasId);
            return;
        }

        var prefs = await _preferencesCache.GetOrDefaultAsync(recipientAliasId, context.CancellationToken);
        if (!prefs.IsTypeEnabled(NotificationType.Comment))
        {
            _logger.LogDebug("Recipient {Recipient} disabled Comment notifications.", recipientAliasId);
            return;
        }

        var source = new NotificationSource
        {
            PostId = msg.PostId,
            CommentId = msg.CommentId,
            Snippet = msg.CommentSnippet 
        };

        // Merge nếu trong 30s đã có 1 notif cùng groupingKey
        var merged = await _notificationRepo.TryMergeLatestAsync(
            recipientAliasId: recipientAliasId,
            groupingKey: groupingKey,
            updater: n =>
            {
                n.ActorAliasId = msg.CommentAuthorAliasId;
                n.ActorDisplayName = msg.CommentAuthorLabel;
                n.Snippet = source.Snippet;
                n.CommentId = msg.CommentId;

                // Giữ CreatedAt cũ để ổn định order; nếu muốn “đẩy lên trên”, có thể cập nhật UpdatedAt (nếu có)
                n.CreatedAt = n.CreatedAt;
            },
            window: TimeSpan.FromSeconds(30),
            ct: context.CancellationToken
        );

        if (!merged)
        {
            // Tạo notif mới
            var notification = UserNotification.Create(
                recipientAliasId: recipientAliasId,
                actorAliasId: msg.CommentAuthorAliasId,
                actorDisplayName: msg.CommentAuthorLabel,
                type: NotificationType.Comment,
                source: source,
                groupingKey: groupingKey
            );

            // Dùng timestamp từ event nguồn để đồng bộ thời gian cross-service (nếu event có)
            // Nếu IntegrationEvent của bạn có CommentedAt/CreatedAt, dùng nó. 
            // Nếu không có, tạm dùng UtcNow hoặc để như mặc định trong Create().
            // notification.CreatedAt = msg.CommentedAt;

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
                FollowId: notification.FollowId,
                ModerationAction: notification.ModerationAction,
                Snippet: notification.Snippet,
                CreatedAt: notification.CreatedAt
            );

            await context.Publish(createdEvt, context.CancellationToken);
            _logger.LogInformation("Created & published comment notification {Id} for {Recipient}", notification.Id,
                recipientAliasId);
        }
        else
        {
            _logger.LogInformation("Merged comment notification for {Recipient} G={Grouping}", recipientAliasId, groupingKey);
        }
    }
}