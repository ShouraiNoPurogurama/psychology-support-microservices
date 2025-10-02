using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.ViewerFollowing;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.Consumers.Alias;

/// <summary>
/// Consumes AliasUnfollowedIntegrationEvent and updates the social graph.
/// Removes the relationship from both tables:
/// - follows_by_viewer (viewer's following list)
/// - followers_of_alias (author's follower list)
/// </summary>
public sealed class AliasUnfollowedIntegrationEventConsumer : IConsumer<AliasUnfollowedIntegrationEvent>
{
    private readonly IViewerFollowingRepository _viewerFollowingRepo;
    private readonly IFollowerTrackingRepository _followerTrackingRepo;
    private readonly ILogger<AliasUnfollowedIntegrationEventConsumer> _logger;

    public AliasUnfollowedIntegrationEventConsumer(
        IViewerFollowingRepository viewerFollowingRepo,
        IFollowerTrackingRepository followerTrackingRepo,
        ILogger<AliasUnfollowedIntegrationEventConsumer> logger)
    {
        _viewerFollowingRepo = viewerFollowingRepo;
        _followerTrackingRepo = followerTrackingRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AliasUnfollowedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing AliasUnfollowed event: {Follower} unfollowed {Unfollowed}",
            message.FollowerAliasId,
            message.UnfollowedAliasId);

        try
        {
            // Remove from both tables to maintain consistency
            var viewerTask = _viewerFollowingRepo.RemoveAsync(
                message.FollowerAliasId,
                message.UnfollowedAliasId,
                context.CancellationToken);

            var followerTask = _followerTrackingRepo.RemoveAsync(
                message.UnfollowedAliasId,
                message.FollowerAliasId,
                context.CancellationToken);

            await Task.WhenAll(viewerTask, followerTask);

            _logger.LogInformation(
                "Successfully processed AliasUnfollowed event: {Follower} -X- {Unfollowed}",
                message.FollowerAliasId,
                message.UnfollowedAliasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing AliasUnfollowed event: {Follower} -X- {Unfollowed}",
                message.FollowerAliasId,
                message.UnfollowedAliasId);
            throw;
        }
    }
}
