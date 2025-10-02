using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.RankingService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes ReactionAddedIntegrationEvent and updates post ranking.
/// Increments reaction count which affects the post's score in feeds.
/// </summary>
public sealed class ReactionAddedIntegrationEventConsumer : IConsumer<ReactionAddedIntegrationEvent>
{
    private readonly IRankingService _rankingService;
    private readonly ILogger<ReactionAddedIntegrationEventConsumer> _logger;

    public ReactionAddedIntegrationEventConsumer(
        IRankingService rankingService,
        ILogger<ReactionAddedIntegrationEventConsumer> logger)
    {
        _rankingService = rankingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReactionAddedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing ReactionAdded event for post {PostId}",
            message.PostId);

        try
        {
            // Increment reaction count in ranking cache
            await _rankingService.IncrementReactionsAsync(
                message.PostId, 
                delta: 1, 
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully updated ranking for post {PostId} after reaction",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing ReactionAdded event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
