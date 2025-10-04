namespace Feed.Application.Abstractions.PostRepository;

/// <summary>
/// Repository for post replica operations in Cassandra.
/// Handles multi-table writes and optimized reads for post data replication.
/// </summary>
public interface IPostReplicaRepository
{
    /// <summary>
    /// Write a published public/finalized post to all three replica tables.
    /// Atomic operation that writes to:
    /// - posts_replica (general data)
    /// - posts_public_finalized_by_day (pre-filtered query table)
    /// - post_replica_by_id (lookup table)
    /// </summary>
    /// <param name="postId">Post identifier</param>
    /// <param name="authorAliasId">Author alias identifier</param>
    /// <param name="visibility">Post visibility (e.g., "Public")</param>
    /// <param name="status">Post status (e.g., "Finalized")</param>
    /// <param name="ymdBucket">Day bucket for partitioning (defaults to today)</param>
    /// <param name="createdAt">Creation timestamp as TimeUUID (defaults to new GUID)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> AddPublicFinalizedPostAsync(
        Guid postId,
        Guid authorAliasId,
        string visibility,
        string status,
        DateOnly? ymdBucket = null,
        DateTimeOffset? createdAt = null,
        CancellationToken ct = default);

    /// <summary>
    /// Delete a post from all replica tables.
    /// Uses lookup table to find the full partition key, then performs precise deletes.
    /// </summary>
    /// <param name="postId">Post identifier to delete</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if post was found and deleted</returns>
    Task<bool> DeletePostReplicaAsync(Guid postId, CancellationToken ct = default);

    /// <summary>
    /// Get the most recent public, finalized posts from the optimized query table.
    /// Used as database fallback when Redis caches are empty.
    /// </summary>
    /// <param name="days">Number of days to query back (default 7)</param>
    /// <param name="limit">Maximum number of posts to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of post IDs ordered by creation date descending</returns>
    Task<IReadOnlyList<Guid>> GetMostRecentPublicPostsAsync(
        int days = 7,
        int limit = 500,
        CancellationToken ct = default);

    /// <summary>
    /// Get post replica lookup information by post ID.
    /// Used internally for delete operations.
    /// </summary>
    /// <param name="postId">Post identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lookup information if found, null otherwise</returns>
    Task<(DateOnly YmdBucket, Guid CreatedAt)?> GetPostLookupAsync(Guid postId, CancellationToken ct = default);
}
