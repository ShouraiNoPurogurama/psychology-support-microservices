namespace Feed.Application.Abstractions.PostRepository;

public record PostInfo(Guid PostId, Guid AuthorAliasId, DateTimeOffset CreatedAt);

/// <summary>
/// Repository for reading post data from PostgreSQL database.
/// Used as the deepest fallback tier when Redis caches are empty.
/// </summary>
public interface IPostReadRepository
{
    /// <summary>
    /// Get the most recent public posts from the database.
    /// This is a slow path and should only be used as last resort fallback.
    /// </summary>
    /// <param name="days">Number of most recent days to find feeds from.</param>
    /// <param name="limit">Maximum number of posts to retrieve</param>
    /// <param name="startDayOffset">Start date to go retrieve feed</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of post IDs ordered by creation date descending</returns>
    Task<IReadOnlyList<PostInfo>> GetMostRecentPublicPostsAsync(int days = 7,
        int limit = 500,
        int startDayOffset = 0,
        CancellationToken cancellationToken = default
        );
}
