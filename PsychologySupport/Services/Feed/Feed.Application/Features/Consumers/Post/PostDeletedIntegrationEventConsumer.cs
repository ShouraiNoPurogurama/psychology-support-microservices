using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostModeration;
using Feed.Domain.PostModeration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostDeletedIntegrationEvent and suppresses the post from all feeds.
/// Adds the post ID to the post_suppressed table to filter it out at read time.
/// </summary>
public sealed class PostDeletedIntegrationEventConsumer : IConsumer<PostDeletedIntegrationEvent>
{
    private readonly IPostModerationRepository _moderationRepo;
    private readonly ILogger<PostDeletedIntegrationEventConsumer> _logger;

    public PostDeletedIntegrationEventConsumer(
        IPostModerationRepository moderationRepo,
        ILogger<PostDeletedIntegrationEventConsumer> logger)
    {
        _moderationRepo = moderationRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostDeletedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostDeleted event for post {PostId} deleted by {DeletedBy}",
            message.PostId,
            message.DeletedByAliasId);

        try
        {
            var postSuppression = PostSuppression.Create(message.PostId, "DELETED", message.DeletedAt);
            
            // Suppress the post by adding it to the suppression table
            await _moderationRepo.SuppressAsync(
                postSuppression, CancellationToken.None);

            _logger.LogInformation(
                "Successfully suppressed deleted post {PostId}",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostDeleted event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
