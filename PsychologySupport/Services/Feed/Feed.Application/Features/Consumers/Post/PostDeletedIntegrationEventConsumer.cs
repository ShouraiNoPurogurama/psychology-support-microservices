using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostModeration;
using Feed.Application.Abstractions.PostRepository;
using Feed.Domain.PostModeration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostDeletedIntegrationEvent and suppresses the post from all feeds.
/// Adds the post ID to the post_suppressed table to filter it out at read time.
/// Also deletes the post from Cassandra replica tables.
/// </summary>
public sealed class PostDeletedIntegrationEventConsumer : IConsumer<PostDeletedIntegrationEvent>
{
    private readonly IPostModerationRepository _moderationRepo;
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostDeletedIntegrationEventConsumer> _logger;

    public PostDeletedIntegrationEventConsumer(
        IPostModerationRepository moderationRepo,
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostDeletedIntegrationEventConsumer> logger)
    {
        _moderationRepo = moderationRepo;
        _postReplicaRepository = postReplicaRepository;
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
                postSuppression, context.CancellationToken);

            _logger.LogInformation(
                "Successfully suppressed deleted post {PostId}",
                message.PostId);

            // Delete from Cassandra replica tables
            var deleted = await _postReplicaRepository.DeletePostReplicaAsync(
                message.PostId,
                context.CancellationToken);

            if (deleted)
            {
                _logger.LogInformation(
                    "Successfully deleted post {PostId} from Cassandra replica tables",
                    message.PostId);
            }
            else
            {
                _logger.LogWarning(
                    "Post {PostId} not found in Cassandra replica tables (may not have been published)",
                    message.PostId);
            }
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
