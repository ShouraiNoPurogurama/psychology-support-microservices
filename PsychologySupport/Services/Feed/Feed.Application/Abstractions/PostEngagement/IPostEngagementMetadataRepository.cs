using Feed.Domain.PostReplica;

namespace Feed.Application.Abstractions.PostEngagement;

/// <summary>
/// Repository interface for managing post engagement metadata.
/// </summary>
public interface IPostEngagementMetadataRepository
{
    /// <summary>
    /// Creates or updates post engagement metadata.
    /// </summary>
    Task<bool> UpsertMetadataAsync(
        PostEngagementMetadata metadata,
        CancellationToken ct = default);

    /// <summary>
    /// Gets post engagement metadata by post ID.
    /// </summary>
    Task<PostEngagementMetadata?> GetMetadataAsync(
        Guid postId,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the counters last updated timestamp.
    /// </summary>
    Task<bool> UpdateCountersTimestampAsync(
        Guid postId,
        DateTimeOffset timestamp,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes post engagement metadata.
    /// </summary>
    Task<bool> DeleteMetadataAsync(
        Guid postId,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if metadata exists for a post.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid postId,
        CancellationToken ct = default);
}
