using BuildingBlocks.Constants;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Moderation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Post.Application.Data;

namespace Post.Application.Features.Posts.EventHandlers;

/// <summary>
/// Handles PostModerationEvaluatedIntegrationEvent from AIModeration service
/// Updates post moderation status based on AI evaluation
/// </summary>
public class PostModerationEvaluatedIntegrationEventHandler : IConsumer<PostModerationEvaluatedIntegrationEvent>
{
    private readonly IPostDbContext _context;
    private readonly ILogger<PostModerationEvaluatedIntegrationEventHandler> _logger;

    public PostModerationEvaluatedIntegrationEventHandler(
        IPostDbContext context,
        ILogger<PostModerationEvaluatedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostModerationEvaluatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing moderation result for PostId: {PostId}, Status: {Status}",
            message.PostId,
            message.Status);

        try
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == message.PostId, context.CancellationToken);

            if (post == null)
            {
                _logger.LogWarning("Post not found: {PostId}", message.PostId);
                return;
            }

            // Update post moderation status based on evaluation
            var systemModeratorId = SystemActors.SystemModeratorUUID;

            switch (message.Status.ToLower())
            {
                case "approved":
                    post.Approve(message.PolicyVersion, systemModeratorId);
                    _logger.LogInformation("Post {PostId} approved", message.PostId);
                    break;

                case "rejected":
                    post.Reject(message.Reasons, message.PolicyVersion, systemModeratorId);
                    _logger.LogInformation(
                        "Post {PostId} rejected. Reasons: {Reasons}",
                        message.PostId,
                        string.Join(", ", message.Reasons));
                    break;

                case "flagged":
                    // For flagged posts, we might want to keep them in pending state for manual review
                    _logger.LogInformation(
                        "Post {PostId} flagged for manual review. Reasons: {Reasons}",
                        message.PostId,
                        string.Join(", ", message.Reasons));
                    break;

                default:
                    _logger.LogWarning(
                        "Unknown moderation status {Status} for PostId: {PostId}",
                        message.Status,
                        message.PostId);
                    break;
            }

            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Successfully processed moderation result for PostId: {PostId}",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing moderation result for PostId: {PostId}",
                message.PostId);
            throw; // Re-throw to allow MassTransit retry logic
        }
    }
}
