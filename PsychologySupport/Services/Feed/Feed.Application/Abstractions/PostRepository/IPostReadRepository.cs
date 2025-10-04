namespace Feed.Application.Abstractions.PostRepository;

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
    /// <param name="limit">Maximum number of posts to retrieve</param>
    /// <returns>List of post IDs ordered by creation date descending</returns>
    Task<IReadOnlyList<Guid>> GetMostRecentPublicPostsAsync(int limit);
}
