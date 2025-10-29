using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using RealtimeHub.API.Models;
using RealtimeHub.API.Services;

namespace RealtimeHub.API.Consumers;

/// <summary>
/// Consumes RewardFailedNotificationCreatedEvent from Notification Service
/// and delivers reward failure notifications to connected clients via SignalR
/// </summary>
public class RewardFailedNotificationCreatedEventConsumer : IConsumer<RewardFailedNotificationCreatedEvent>
{
    private readonly IRealtimeHubService _realtimeHubService;
    private readonly ILogger<RewardFailedNotificationCreatedEventConsumer> _logger;

    public RewardFailedNotificationCreatedEventConsumer(
        IRealtimeHubService realtimeHubService,
        ILogger<RewardFailedNotificationCreatedEventConsumer> logger)
    {
        _realtimeHubService = realtimeHubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RewardFailedNotificationCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received RewardFailedNotificationCreatedEvent for notification {NotificationId}, recipient {RecipientId}, reward {RewardId}",
            message.NotificationId, message.RecipientAliasId, message.RewardId);

        // Map to RewardFailedNotificationMessage
        var rewardFailedNotificationMessage = new RewardFailedNotificationMessage
        {
            NotificationId = message.NotificationId,
            RecipientAliasId = message.RecipientAliasId,
            RewardId = message.RewardId,
            SessionId = message.SessionId,
            ErrorMessage = message.ErrorMessage,
            PointsRefunded = message.PointsRefunded,
            Snippet = message.Snippet,
            CreatedAt = message.CreatedAt
        };

        // Send to user via SignalR using the reward failed notification method
        await _realtimeHubService.SendRewardFailedNotificationToUserAsync(
            message.RecipientAliasId,
            rewardFailedNotificationMessage,
            context.CancellationToken);

        _logger.LogInformation(
            "Sent reward failure notification {NotificationId} to user {RecipientId} via SignalR",
            message.NotificationId, message.RecipientAliasId);
    }
}
