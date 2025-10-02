using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostModeration;
using Feed.Domain.PostModeration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes ModerationEvaluatedIntegrationEvent and updates post suppression status.
/// When moderation rejects a post, it should be suppressed from feeds.
/// When moderation approves a post, any existing suppression should be removed.
/// </summary>
public sealed class ModerationEvaluatedIntegrationEventConsumer : IConsumer<ModerationEvaluatedIntegrationEvent>
{
    private readonly IPostModerationRepository _moderationRepo;
    private readonly ILogger<ModerationEvaluatedIntegrationEventConsumer> _logger;

    public ModerationEvaluatedIntegrationEventConsumer(
        IPostModerationRepository moderationRepo,
        ILogger<ModerationEvaluatedIntegrationEventConsumer> logger)
    {
        _moderationRepo = moderationRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ModerationEvaluatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing ModerationEvaluated event for post {PostId}: Decision={Decision}, Reason={Reason}",
            message.PostId,
            message.Decision,
            message.Reason);

        try
        {
            if (message.Decision == ModerationDecision.Rejected)
            {
                // Suppress the post from feeds
                var postSuppression = PostSuppression.Create(
                    message.PostId,
                    $"MODERATED: {message.Reason ?? "Content policy violation"}",
                    DateTimeOffset.UtcNow);

                await _moderationRepo.SuppressAsync(
                    postSuppression,
                    context.CancellationToken);

                _logger.LogInformation(
                    "Successfully suppressed rejected post {PostId}",
                    message.PostId);
            }
            else if (message.Decision == ModerationDecision.Approved)
            {
                // Remove any existing suppression
                var unsuppressed = await _moderationRepo.UnsuppressAsync(
                    message.PostId,
                    context.CancellationToken);

                if (unsuppressed)
                {
                    _logger.LogInformation(
                        "Successfully unsuppressed approved post {PostId}",
                        message.PostId);
                }
                else
                {
                    _logger.LogDebug(
                        "Post {PostId} was not suppressed, no action needed",
                        message.PostId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing ModerationEvaluated event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
