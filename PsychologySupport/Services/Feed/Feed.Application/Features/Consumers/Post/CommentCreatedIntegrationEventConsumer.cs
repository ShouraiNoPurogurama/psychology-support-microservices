using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.RankingService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes CommentCreatedIntegrationEvent and updates post ranking.
/// Increments comment count which affects the post's score in feeds.
/// Comments typically have higher weight than reactions in ranking algorithms.
/// </summary>
public sealed class CommentCreatedIntegrationEventConsumer : IConsumer<CommentCreatedIntegrationEvent>
{
    private readonly IRankingService _rankingService;
    private readonly ILogger<CommentCreatedIntegrationEventConsumer> _logger;

    public CommentCreatedIntegrationEventConsumer(
        IRankingService rankingService,
        ILogger<CommentCreatedIntegrationEventConsumer> logger)
    {
        _rankingService = rankingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommentCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CommentCreated event for post {PostId}",
            message.PostId);

        try
        {
            // Increment comment count in ranking cache
            // Comments typically have higher weight than reactions
            await _rankingService.IncrementCommentsAsync(
                message.PostId, 
                delta: 1, 
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully updated ranking for post {PostId} after comment",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing CommentCreated event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
