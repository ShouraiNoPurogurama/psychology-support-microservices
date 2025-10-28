using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using MassTransit;
using RealtimeHub.API.Models;
using RealtimeHub.API.Services;

namespace RealtimeHub.API.Consumers;

/// <summary>
/// Consumes ProgressUpdatedIntegrationEvent from UserMemory Service
/// and delivers progress updates to connected clients via SignalR in real-time.
/// </summary>
public class ProgressUpdatedIntegrationEventConsumer : IConsumer<ProgressUpdatedIntegrationEvent>
{
    private readonly IRealtimeHubService _realtimeHubService;
    private readonly ILogger<ProgressUpdatedIntegrationEventConsumer> _logger;

    public ProgressUpdatedIntegrationEventConsumer(
        IRealtimeHubService realtimeHubService,
        ILogger<ProgressUpdatedIntegrationEventConsumer> logger)
    {
        _realtimeHubService = realtimeHubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProgressUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ProgressUpdatedIntegrationEvent for AliasId={AliasId}, PointsEarned={Points}, TotalDaily={TotalDaily}",
            message.AliasId, message.PointsEarned, message.TotalDailyPoints);

        // Map to ProgressUpdateMessage
        var progressUpdateMessage = new ProgressUpdateMessage
        {
            AliasId = message.AliasId,
            SessionId = message.SessionId,
            PointsEarned = message.PointsEarned,
            TotalSessionPoints = message.TotalSessionPoints,
            TotalDailyPoints = message.TotalDailyPoints,
            Achievement = message.Achievement,
            UpdatedAt = message.UpdatedAt
        };

        // Send to user via SignalR
        await _realtimeHubService.SendProgressUpdateToUserAsync(
            message.AliasId,
            progressUpdateMessage,
            context.CancellationToken);

        _logger.LogInformation(
            "Sent progress update to user {AliasId} via SignalR: +{Points} points",
            message.AliasId, message.PointsEarned);
    }
}
