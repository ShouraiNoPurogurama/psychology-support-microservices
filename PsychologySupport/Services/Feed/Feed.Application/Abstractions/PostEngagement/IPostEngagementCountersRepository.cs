using Feed.Domain.PostReplica;

namespace Feed.Application.Abstractions.PostEngagement;

/// <summary>
/// Repository interface for managing post engagement counters.
/// Handles counter increments and reads for engagement metrics.
/// </summary>
public interface IPostEngagementCountersRepository
{
    /// <summary>
    /// Increments the reactions counter for a post.
    /// </summary>
    Task<bool> IncrementReactionsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the comments counter for a post.
    /// </summary>
    Task<bool> IncrementCommentsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the shares counter for a post.
    /// </summary>
    Task<bool> IncrementSharesAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the clicks counter for a post.
    /// </summary>
    Task<bool> IncrementClicksAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the impressions counter for a post.
    /// </summary>
    Task<bool> IncrementImpressionsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Increments the view duration counter for a post.
    /// </summary>
    Task<bool> IncrementViewDurationAsync(
        Guid postId,
        long seconds,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the engagement counters for a post.
    /// </summary>
    Task<PostEngagementCounters?> GetCountersAsync(
        Guid postId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets engagement counters for multiple posts in batch.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, PostEngagementCounters>> GetCountersBatchAsync(
        IEnumerable<Guid> postIds,
        CancellationToken ct = default);

    /// <summary>
    /// Resets all counters for a post (deletes the row).
    /// Note: In Cassandra, you cannot set counters to specific values, only increment or delete.
    /// </summary>
    Task<bool> DeleteCountersAsync(
        Guid postId,
        CancellationToken ct = default);
}
