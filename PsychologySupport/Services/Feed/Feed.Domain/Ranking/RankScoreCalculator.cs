namespace Feed.Domain.Ranking;

/// <summary>
/// Calculates deterministic ranking scores for feed items.
/// Initial algorithm: rank_bucket = 0, rank_i64 = (MaxEpochMillis - createdAtMillis)
/// This provides recency-first ranking that's deterministic and idempotent.
/// </summary>
public static class RankScoreCalculator
{
    private const long MaxEpochMillis = long.MaxValue;

    /// <summary>
    /// Calculates the ranking bucket. Currently always returns 0 (recency-only).
    /// </summary>
    public static sbyte CalculateRankBucket()
    {
        return 0;
    }

    /// <summary>
    /// Calculates the rank score based on post creation time.
    /// Higher values = older posts (sort descending for newest first).
    /// Formula: MaxValue - createdAtMillis for deterministic idempotent ranking.
    /// </summary>
    public static long CalculateRankI64(DateTimeOffset createdAt)
    {
        var millis = createdAt.ToUnixTimeMilliseconds();
        return MaxEpochMillis - millis;
    }

    /// <summary>
    /// Calculates rank score from ticks for backward compatibility.
    /// </summary>
    public static long CalculateRankI64FromTicks(long ticks)
    {
        return long.MaxValue - ticks;
    }

    /// <summary>
    /// Creates a deterministic TimeUuid from a timestamp.
    /// For idempotency, we use the post creation time to generate consistent UUIDs.
    /// </summary>
    public static Guid CreateDeterministicTimeUuid(DateTimeOffset createdAt, Guid postId)
    {
        // Combine timestamp and postId for deterministic UUID generation
        // This ensures the same post at the same time generates the same UUID
        var timeBytes = BitConverter.GetBytes(createdAt.UtcTicks);
        var postBytes = postId.ToByteArray();
        
        var combined = new byte[16];
        Array.Copy(timeBytes, 0, combined, 0, 8);
        Array.Copy(postBytes, 0, combined, 8, 8);
        
        return new Guid(combined);
    }
}
