using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.PostRepository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Post;

/// <summary>
/// Consumes PostApprovedIntegrationEvent when a post passes moderation.
/// If the post is public, it's added to the Cassandra replica tables for feed distribution.
/// </summary>
public sealed class PostApprovedIntegrationEventConsumer : IConsumer<PostApprovedIntegrationEvent>
{
    private readonly IPostReplicaRepository _postReplicaRepository;
    private readonly ILogger<PostApprovedIntegrationEventConsumer> _logger;

    public PostApprovedIntegrationEventConsumer(
        IPostReplicaRepository postReplicaRepository,
        ILogger<PostApprovedIntegrationEventConsumer> logger)
    {
        _postReplicaRepository = postReplicaRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostApprovedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PostApproved event for post {PostId} by moderator {ModeratorId}",
            message.PostId,
            message.ModeratorAliasId);

        try
        {
            // Add approved public post to Cassandra replica tables
            // Assuming approved posts are public and finalized
            await _postReplicaRepository.AddPublicFinalizedPostAsync(
                message.PostId,
                message.AuthorAliasId,
                visibility: "Public",
                status: "Approved",
                ymdBucket: DateOnly.FromDateTime(message.ApprovedAt.Date),
                createdAt: null, // Will use time-ordered GUID
                ct: context.CancellationToken);

            _logger.LogInformation(
                "Successfully added approved post {PostId} to Cassandra replica tables",
                message.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing PostApproved event for post {PostId}",
                message.PostId);
            throw;
        }
    }
}
