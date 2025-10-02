using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Domain.FollowerTracking;
using Feed.Domain.ViewerFollowing;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Alias;

/// <summary>
/// Consumes AliasFollowedIntegrationEvent and updates the social graph.
/// Maintains bidirectional follow relationships in both tables:
/// - follows_by_viewer (for viewer's following list)
/// - followers_of_alias (for author's follower list)
/// </summary>
public sealed class AliasFollowedIntegrationEventConsumer : IConsumer<AliasFollowedIntegrationEvent>
{
    private readonly IViewerFollowingRepository _viewerFollowingRepo;
    private readonly IFollowerTrackingRepository _followerTrackingRepo;
    private readonly ILogger<AliasFollowedIntegrationEventConsumer> _logger;

    public AliasFollowedIntegrationEventConsumer(
        IViewerFollowingRepository viewerFollowingRepo,
        IFollowerTrackingRepository followerTrackingRepo,
        ILogger<AliasFollowedIntegrationEventConsumer> logger)
    {
        _viewerFollowingRepo = viewerFollowingRepo;
        _followerTrackingRepo = followerTrackingRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AliasFollowedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing AliasFollowed event: {Follower} followed {Followed}",
            message.FollowerAliasId,
            message.FollowedAliasId);

        try
        {
            // Create ViewerFollow entity for follows_by_viewer table
            var viewerFollow = ViewerFollow.Create(
                message.FollowerAliasId,
                message.FollowedAliasId,
                message.FollowedAt);

            // Create Follower entity for followers_of_alias table
            // Parameters: (followerAliasId, aliasId being followed, since)
            var follower = Follower.Create(
                message.FollowerAliasId,
                message.FollowedAliasId,
                message.FollowedAt);

            // Insert into both tables to maintain bidirectional relationship
            var viewerTask = _viewerFollowingRepo.AddIfNotExistsAsync(viewerFollow, context.CancellationToken);
            var followerTask = _followerTrackingRepo.AddIfNotExistsAsync(follower, context.CancellationToken);

            await Task.WhenAll(viewerTask, followerTask);

            _logger.LogInformation(
                "Successfully processed AliasFollowed event: {Follower} -> {Followed}",
                message.FollowerAliasId,
                message.FollowedAliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing AliasFollowed event: {Follower} -> {Followed}",
                message.FollowerAliasId,
                message.FollowedAliasId);
            throw;
        }
    }
}
