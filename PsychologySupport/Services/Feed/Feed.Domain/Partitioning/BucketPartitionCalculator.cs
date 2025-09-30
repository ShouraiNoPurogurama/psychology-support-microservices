namespace Feed.Domain.Partitioning;

/// <summary>
/// Calculates bucket and shard partitions for feed items.
/// </summary>
public static class BucketPartitionCalculator
{
    /// <summary>
    /// Calculates the YMD bucket for a given date.
    /// </summary>
    public static DateOnly CalculateYmdBucket(DateTimeOffset timestamp)
    {
        return DateOnly.FromDateTime(timestamp.UtcDateTime.Date);
    }

    /// <summary>
    /// Calculates the shard for a feed item using deterministic hashing.
    /// This ensures the same (postId, followerId) combination always produces the same shard.
    /// </summary>
    public static short CalculateShard(Guid postId, Guid followerId, int shardCount)
    {
        if (shardCount <= 0)
            throw new ArgumentException("Shard count must be positive", nameof(shardCount));

        var hash = HashCode.Combine(postId, followerId);
        var idx = Math.Abs(hash % shardCount);
        return (short)idx;
    }

    /// <summary>
    /// Calculates the shard using only the follower ID (for single-entity sharding).
    /// </summary>
    public static short CalculateShardByFollower(Guid followerId, int shardCount)
    {
        if (shardCount <= 0)
            throw new ArgumentException("Shard count must be positive", nameof(shardCount));

        var hash = followerId.GetHashCode();
        var idx = Math.Abs(hash % shardCount);
        return (short)idx;
    }
}
