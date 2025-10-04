using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostRepository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostVisibilityUpdatedIntegrationEvent and updates the post in Cassandra replica tables.
/// If the post becomes public, it's added to the public_finalized query table.
/// If it becomes private, it's removed from the public_finalized query table.
/// </summary>
public sealed class PostVisibilityUpdatedIntegrationEventConsumer : IConsumer<PostVisibilityUpdatedIntegrationEvent>
{
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostVisibilityUpdatedIntegrationEventConsumer> _logger;

    public PostVisibilityUpdatedIntegrationEventConsumer(
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostVisibilityUpdatedIntegrationEventConsumer> logger)
    {
        _postReplicaRepository = postReplicaRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostVisibilityUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostVisibilityUpdated event for post {PostId}: {OldVisibility} -> {NewVisibility}",
            message.PostId,
            message.OldVisibility,
            message.NewVisibility);

        try
        {
            // If the post is transitioning to public and is finalized, add it to replica tables
            if (message.NewVisibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Post {PostId} became public, adding to Cassandra replica tables",
                    message.PostId);

                await _postReplicaRepository.AddPublicFinalizedPostAsync(
                    message.PostId,
                    message.AuthorAliasId,
                    visibility: message.NewVisibility,
                    status: "Finalized", // Assuming published posts are finalized
                    ymdBucket: DateOnly.FromDateTime(message.UpdatedAt.Date),
                    createdAt: null, // Will use time-ordered GUID
                    ct: context.CancellationToken);

                _logger.LogInformation(
                    "Successfully added public post {PostId} to Cassandra replica tables",
                    message.PostId);
            }
            // If the post is transitioning from public to private, remove it from public_finalized table
            else if (message.OldVisibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Post {PostId} became non-public, removing from Cassandra replica tables",
                    message.PostId);

                var deleted = await _postReplicaRepository.DeletePostReplicaAsync(
                    message.PostId,
                    context.CancellationToken);

                if (deleted)
                {
                    _logger.LogInformation(
                        "Successfully removed post {PostId} from Cassandra replica tables",
                        message.PostId);
                }
                else
                {
                    _logger.LogWarning(
                        "Post {PostId} not found in Cassandra replica tables during visibility change",
                        message.PostId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostVisibilityUpdated event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
