using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.API.Contracts;

namespace Notification.API.Features.Notifications.Consumers;

public class PostDeletedIntegrationEventConsumer : IConsumer<PostDeletedIntegrationEvent>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IProcessedEventRepository _processedEventRepo;
    private readonly ILogger<PostDeletedIntegrationEventConsumer> _logger;

    public PostDeletedIntegrationEventConsumer(
        INotificationRepository notificationRepo,
        IProcessedEventRepository processedEventRepo,
        ILogger<PostDeletedIntegrationEventConsumer> logger)
    {
        _notificationRepo = notificationRepo;
        _processedEventRepo = processedEventRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostDeletedIntegrationEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();

        _logger.LogInformation(
            "Processing PostDeleted event {MessageId} for post {PostId}",
            messageId, message.PostId);

        // Idempotency check
        if (!await _processedEventRepo.TryAddAsync(messageId, nameof(PostDeletedIntegrationEvent), context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        // Delete all notifications related to this post
        var deletedCount = await _notificationRepo.DeleteBySourceAsync(
            postId: message.PostId,
            cancellationToken: context.CancellationToken);

        _logger.LogInformation(
            "Deleted {Count} notifications for deleted post {PostId}",
            deletedCount, message.PostId);
    }
}
