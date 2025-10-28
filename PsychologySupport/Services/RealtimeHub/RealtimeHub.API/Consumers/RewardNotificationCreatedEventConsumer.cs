using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using RealtimeHub.API.Models;
using RealtimeHub.API.Services;

namespace RealtimeHub.API.Consumers;

/// <summary>
/// Consumes RewardNotificationCreatedEvent from Notification Service
/// and delivers reward notifications to connected clients via SignalR
/// </summary>
public class RewardNotificationCreatedEventConsumer : IConsumer<RewardNotificationCreatedEvent>
{
    private readonly IRealtimeHubService _realtimeHubService;
    private readonly ILogger<RewardNotificationCreatedEventConsumer> _logger;

    public RewardNotificationCreatedEventConsumer(
        IRealtimeHubService realtimeHubService,
        ILogger<RewardNotificationCreatedEventConsumer> logger)
    {
        _realtimeHubService = realtimeHubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RewardNotificationCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received RewardNotificationCreatedEvent for notification {NotificationId}, recipient {RecipientId}, reward {RewardId}",
            message.NotificationId, message.RecipientAliasId, message.RewardId);

        // Map to RewardNotificationMessage
        var rewardNotificationMessage = new RewardNotificationMessage
        {
            NotificationId = message.NotificationId,
            RecipientAliasId = message.RecipientAliasId,
            RewardId = message.RewardId,
            SessionId = message.SessionId,
            StickerUrl = message.StickerUrl,
            Snippet = message.Snippet,
            CreatedAt = message.CreatedAt
        };

        // Send to user via SignalR using the specific reward notification method
        await _realtimeHubService.SendRewardNotificationToUserAsync(
            message.RecipientAliasId,
            rewardNotificationMessage,
            context.CancellationToken);

        _logger.LogInformation(
            "Sent reward notification {NotificationId} to user {RecipientId} via SignalR",
            message.NotificationId, message.RecipientAliasId);
    }
}
