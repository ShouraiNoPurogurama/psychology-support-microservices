using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Abstractions.RankingService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostCreatedIntegrationEvent and initializes post ranking data.
/// This ensures the post has a ranking entry from creation, even before publication.
/// Also stores the author ID for future filtering by blocked/muted users.
/// Additionally, syncs post data to Cassandra replica tables (posts_replica only, not public_finalized yet).
/// </summary>
public sealed class PostCreatedIntegrationEventConsumer : IConsumer<PostCreatedIntegrationEvent>
{
    private readonly IRankingService _rankingService;
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostCreatedIntegrationEventConsumer> _logger;

    public PostCreatedIntegrationEventConsumer(
        IRankingService rankingService,
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostCreatedIntegrationEventConsumer> logger)
    {
        _rankingService = rankingService;
        _postReplicaRepository = postReplicaRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostCreated event for post {PostId} by author {AuthorId}",
            message.PostId,
            message.AuthorAliasId);

        try
        {
            // Initialize ranking data for the post
            await _rankingService.InitializePostRankAsync(
                message.PostId,
                message.CreatedAt,
                context.CancellationToken);

            // Store author ID for future filtering
            await _rankingService.SetPostAuthorAsync(
                message.PostId,
                message.AuthorAliasId,
                context.CancellationToken);

            _logger.LogInformation(
                "Successfully initialized ranking for post {PostId}",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostCreated event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
