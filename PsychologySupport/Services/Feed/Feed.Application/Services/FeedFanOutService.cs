using Feed.Application.Abstractions.FanOut;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Configuration;
using Feed.Domain.Partitioning;
using Feed.Domain.Ranking;
using Feed.Domain.UserFeed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feed.Application.Services;

/// <summary>
/// Service for fan-out operations to distribute posts to followers' feeds.
/// Implements write-heavy fan-out for VIP users with many followers.
/// </summary>
public sealed class FeedFanOutService : IFeedFanOutService
{
    private readonly IFollowerTrackingRepository _followerRepo;
    private readonly IUserFeedRepository _feedRepo;
    private readonly FeedConfiguration _config;
    private readonly ILogger<FeedFanOutService> _logger;

    public FeedFanOutService(
        IFollowerTrackingRepository followerRepo,
        IUserFeedRepository feedRepo,
        IOptions<FeedConfiguration> options,
        ILogger<FeedFanOutService> logger)
    {
        _followerRepo = followerRepo;
        _feedRepo = feedRepo;
        _config = options.Value;
        _logger = logger;
    }

    public async Task FanOutPostAsync(
        Guid postId,
        Guid authorAliasId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        // Get all followers of the author
        var followers = await _followerRepo.GetAllFollowersOfAliasAsync(authorAliasId, cancellationToken);

        if (followers.Count == 0)
        {
            _logger.LogDebug("No followers found for author {AuthorId}, skipping fan-out for post {PostId}",
                authorAliasId, postId);
            return;
        }

        _logger.LogInformation("Fanning out post {PostId} to {Count} followers of author {AuthorId}",
            postId, followers.Count, authorAliasId);

        // Calculate deterministic ranking values for idempotency
        var ymdBucket = BucketPartitionCalculator.CalculateYmdBucket(createdAt);
        var rankBucket = RankScoreCalculator.CalculateRankBucket();
        var rankI64 = RankScoreCalculator.CalculateRankI64(createdAt);

        // Prepare fan-out tasks
        var tasks = new List<Task>(capacity: followers.Count);

        foreach (var follower in followers)
        {
            // Calculate shard deterministically
            var shard = BucketPartitionCalculator.CalculateShard(postId, follower.AliasId, _config.FeedShardCount);
            
            // Create deterministic TimeUuid for idempotency
            var tsUuid = RankScoreCalculator.CreateDeterministicTimeUuid(createdAt, postId);

            var feedItem = UserFeedItem.Create(
                aliasId: follower.AliasId,
                postId: postId,
                ymdBucket: ymdBucket,
                shard: shard,
                rankBucket: rankBucket,
                rankI64: rankI64,
                tsUuid: tsUuid,
                createdAt: createdAt);

            tasks.Add(_feedRepo.AddFeedItemAsync(feedItem, cancellationToken));
        }

        // Execute in batches to avoid overwhelming Cassandra
        const int batchSize = 100;
        for (int i = 0; i < tasks.Count; i += batchSize)
        {
            var batch = tasks.Skip(i).Take(batchSize).ToArray();
            await Task.WhenAll(batch);
        }

        _logger.LogInformation("Successfully fanned out post {PostId} to {Count} followers",
            postId, followers.Count);
    }

    public async Task RemovePostFromFeedsAsync(
        Guid postId,
        Guid authorAliasId,
        CancellationToken cancellationToken)
    {
        // Get all followers who might have this post in their feeds
        var followers = await _followerRepo.GetAllFollowersOfAliasAsync(authorAliasId, cancellationToken);

        if (followers.Count == 0)
        {
            _logger.LogDebug("No followers found for author {AuthorId}, skipping removal for post {PostId}",
                authorAliasId, postId);
            return;
        }

        _logger.LogInformation("Removing post {PostId} from {Count} followers' feeds",
            postId, followers.Count);

        // Note: We need to iterate through potential buckets and shards
        // For simplicity, we'll use the current date and all shards
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tasks = new List<Task>();

        foreach (var follower in followers)
        {
            // Try all shards (we don't know which one was used originally)
            for (short shard = 0; shard < _config.FeedShardCount; shard++)
            {
                tasks.Add(_feedRepo.RemoveFeedItemAsync(
                    follower.AliasId,
                    today,
                    shard,
                    postId,
                    cancellationToken));
            }
        }

        // Execute in batches
        const int batchSize = 100;
        for (int i = 0; i < tasks.Count; i += batchSize)
        {
            var batch = tasks.Skip(i).Take(batchSize).ToArray();
            await Task.WhenAll(batch);
        }

        _logger.LogInformation("Successfully removed post {PostId} from followers' feeds", postId);
    }
}
