using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostModeration;
using Feed.Domain.PostModeration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.EventHandlers.Posts;

public sealed class ModerationEvaluatedIntegrationEventHandler(
    IPostModerationRepository moderationRepository,
    ILogger<ModerationEvaluatedIntegrationEventHandler> logger)
    : IConsumer<ModerationEvaluatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ModerationEvaluatedIntegrationEvent> context)
    {
        var msg = context.Message;
        if (msg.Decision == ModerationDecision.Rejected)
        {
            var suppression = PostSuppression.Create(msg.PostId, msg.Reason);
            await moderationRepository.SuppressAsync(suppression, context.CancellationToken);
            logger.LogInformation("Suppressed post {PostId} due to moderation rejection.", msg.PostId);
        }
        else
        {
            // If approved, ensure it's not suppressed anymore
            await moderationRepository.UnsuppressAsync(msg.PostId, context.CancellationToken);
            logger.LogDebug("Unsuppressed post {PostId} after approval.", msg.PostId);
        }
    }
}

