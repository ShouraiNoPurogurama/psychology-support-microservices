using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Configuration;
using Feed.Domain.UserFeed;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feed.Application.Features.EventHandlers.Posts;

public sealed class PostApprovedIntegrationEventHandler(
    IFollowerTrackingRepository followerRepo,
    IUserFeedRepository feedRepo,
    IOptions<FeedConfiguration> options,
    ILogger<PostApprovedIntegrationEventHandler> logger)
    : IConsumer<PostApprovedIntegrationEvent>
{
    private readonly FeedConfiguration _config = options.Value;

    public async Task Consume(ConsumeContext<PostApprovedIntegrationEvent> context)
    {
        var msg = context.Message;
        var authorId = msg.AuthorAliasId;
        // Determine VIP by follower threshold
        var followers = await followerRepo.GetAllFollowersAsync(authorId, context.CancellationToken);
        var followerCount = followers.Count;

        if (followerCount < _config.VipCriteria.MinFollowers)
        {
            logger.LogDebug("Post {PostId} by {Author} is non-VIP (followers={Count}), skipping fan-out.", msg.PostId, authorId, followerCount);
            return; // Fan-in path: do nothing
        }

        logger.LogInformation("Fan-out post {PostId} by VIP {Author} to {Count} followers.", msg.PostId, authorId, followerCount);

        // Prepare feed items distributed across shards
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tasks = new List<Task>(capacity: followers.Count);

        foreach (var f in followers)
        {
            var shard = ComputeShard(msg.PostId, f.AliasId, _config.FeedShardCount);
            // Recency-first default rank; background jobs will adjust via engagement
            var rankBucket = (sbyte)0;
            var rankI64 = long.MaxValue - DateTime.UtcNow.Ticks;
            var item = UserFeedItem.Create(
                aliasId: f.AliasId,
                postId: msg.PostId,
                ymdBucket: today,
                shard: shard,
                rankBucket: rankBucket,
                rankI64: rankI64,
                tsUuid: Guid.NewGuid(),
                createdAt: msg.ApprovedAt);

            tasks.Add(feedRepo.AddFeedItemAsync(item, context.CancellationToken));
        }

        // Throttle in batches to avoid overwhelming Cassandra coordinator
        const int batchSize = 100;
        for (int i = 0; i < tasks.Count; i += batchSize)
        {
            var batch = tasks.Skip(i).Take(batchSize).ToArray();
            await Task.WhenAll(batch);
        }
    }

    private static short ComputeShard(Guid postId, Guid followerId, int shardCount)
    {
        var hash = HashCode.Combine(postId, followerId);
        var idx = Math.Abs(hash % Math.Max(1, shardCount));
        return (short)idx;
    }
}

