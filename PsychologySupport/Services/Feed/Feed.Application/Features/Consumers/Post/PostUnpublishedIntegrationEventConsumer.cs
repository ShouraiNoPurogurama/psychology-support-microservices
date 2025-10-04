using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostModeration;
using Feed.Application.Abstractions.PostRepository;
using Feed.Domain.PostModeration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostUnpublishedIntegrationEvent and suppresses the post in feeds.
/// When a post is unpublished (made private or hidden), it should be removed from all feeds.
/// This is similar to deletion but potentially temporary.
/// Also removes the post from Cassandra replica tables.
/// </summary>
public sealed class PostUnpublishedIntegrationEventConsumer : IConsumer<PostUnpublishedIntegrationEvent>
{
    private readonly IPostModerationRepository _moderationRepo;
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostUnpublishedIntegrationEventConsumer> _logger;

    public PostUnpublishedIntegrationEventConsumer(
        IPostModerationRepository moderationRepo,
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostUnpublishedIntegrationEventConsumer> logger)
    {
        _moderationRepo = moderationRepo;
        _postReplicaRepository = postReplicaRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostUnpublishedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostUnpublished event for post {PostId} by author {AuthorId}",
            message.PostId,
            message.AuthorAliasId);

        try
        {
            // Suppress the post so it won't appear in feeds
            // Using "UNPUBLISHED" reason to distinguish from deletion
            var postSuppression = PostSuppression.Create(
                message.PostId, 
                "UNPUBLISHED", 
                message.UnpublishedAt);

            await _moderationRepo.SuppressAsync(
                postSuppression, 
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully suppressed unpublished post {PostId}",
                message.PostId);

            // Remove from Cassandra replica tables
            var deleted = await _postReplicaRepository.DeletePostReplicaAsync(
                message.PostId,
                context.CancellationToken);

            if (deleted)
            {
                _logger.LogInformation(
                    "Successfully removed unpublished post {PostId} from Cassandra replica tables",
                    message.PostId);
            }
            else
            {
                _logger.LogWarning(
                    "Post {PostId} not found in Cassandra replica tables during unpublish",
                    message.PostId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostUnpublished event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
