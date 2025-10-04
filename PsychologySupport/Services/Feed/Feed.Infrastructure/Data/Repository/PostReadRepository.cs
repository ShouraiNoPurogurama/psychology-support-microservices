using Feed.Application.Abstractions.PostRepository;

namespace Feed.Infrastructure.Data.Repository;

/// <summary>
/// Stub implementation for IPostReadRepository.
/// This serves as the deepest fallback tier to retrieve posts from the database.
/// </summary>
public sealed class PostReadRepository : IPostReadRepository
{
    private readonly FeedDbContext _dbContext;

    public PostReadRepository(FeedDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get the most recent public posts from the database.
    /// This is a SLOW PATH and should only be used when all Redis caches are empty.
    /// TODO: Implement actual database query when Post replica tables are available.
    /// </summary>
    public Task<IReadOnlyList<Guid>> GetMostRecentPublicPostsAsync(int limit, CancellationToken ct)
    {
        // Stub implementation - returns empty list
        // In production, this would query the post replica table:
        // 
        // var posts = await _dbContext.PostReplicas
        //     .Where(p => p.Visibility == "Public" && p.Status == "Published")
        //     .OrderByDescending(p => p.CreatedAt)
        //     .Take(limit)
        //     .Select(p => p.Id)
        //     .ToListAsync(ct);
        //
        // return posts;

        return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }
}
