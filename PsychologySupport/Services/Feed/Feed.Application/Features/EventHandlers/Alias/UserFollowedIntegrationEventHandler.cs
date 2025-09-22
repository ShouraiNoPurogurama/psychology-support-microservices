using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Domain.FollowerTracking;
using Feed.Domain.ViewerFollowing;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.EventHandlers.Alias;

public sealed class UserFollowedIntegrationEventHandler(
    IFollowerTrackingRepository followerRepo,
    IViewerFollowingRepository followingRepo,
    ILogger<UserFollowedIntegrationEventHandler> logger)
    : IConsumer<UserFollowedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserFollowedIntegrationEvent> context)
    {
        var msg = context.Message;

        // Write to follows_by_viewer and followers_by_alias (idempotent add-if-not-exists)
        var follow = ViewerFollow.Create(msg.FollowerAliasId, msg.FollowedAliasId, msg.Timestamp);
        var follower = Follower.Create(msg.FollowedAliasId, msg.FollowerAliasId, msg.Timestamp);

        await followingRepo.AddIfNotExistsAsync(follow, context.CancellationToken);
        await followerRepo.AddIfNotExistsAsync(follower, context.CancellationToken);

        logger.LogInformation("Recorded follow: {Follower} -> {Followed}", msg.FollowerAliasId, msg.FollowedAliasId);
    }
}

