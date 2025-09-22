using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Features.Ranking;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.EventHandlers.Posts;

public sealed class CommentCreatedIntegrationEventHandler(
    IRankingService rankingService,
    IPublishEndpoint publishEndpoint,
    ILogger<CommentCreatedIntegrationEventHandler> logger)
    : IConsumer<CommentCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CommentCreatedIntegrationEvent> context)
    {
        var msg = context.Message;

        // Lightweight: just increment comments and enqueue background rank update
        await rankingService.IncrementCommentsAsync(msg.PostId, 1, context.CancellationToken);

        await publishEndpoint.Publish(new UpdatePostRankCommand(msg.PostId), context.CancellationToken);
        logger.LogDebug("Queued rank update due to comment for post {PostId}", msg.PostId);
    }
}

