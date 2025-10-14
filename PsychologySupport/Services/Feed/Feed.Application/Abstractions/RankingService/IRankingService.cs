using Feed.Application.Dtos;

namespace Feed.Application.Abstractions.RankingService;

public interface IRankingService
{
    Task<IReadOnlyList<Guid>> GetTrendingPostsAsync(DateTimeOffset date, CancellationToken ct);
    Task UpdatePostRankAsync(Guid postId, PostRankData rankData);

    Task<IReadOnlyList<RankedPost>> RankPostsAsync(IReadOnlyList<Guid> followedAliasIds, IReadOnlyList<Guid> trendingPostIds,
        int limit, CancellationToken ct);

    Task AddToTrendingAsync(Guid postId, double score, DateTimeOffset date, CancellationToken ct);



    // Helpers to support background rank updates and filtering
    Task InitializePostRankAsync(Guid postId, DateTimeOffset createdAt, CancellationToken ct);
    Task<PostRankData?> GetPostRankAsync(Guid postId);
    Task IncrementReactionsAsync(Guid postId, int delta, CancellationToken ct);
    Task IncrementCommentsAsync(Guid postId, int delta, CancellationToken ct);

    // PostId -> AuthorAliasId mapping for filtering
    Task SetPostAuthorAsync(Guid postId, Guid authorAliasId, CancellationToken ct);
    Task<Guid?> GetPostAuthorAsync(Guid postId, CancellationToken ct);

    /// <summary>
    /// Get global fallback posts when daily trending is empty.
    /// Reads from Redis Sorted Set: trending:global_fallback
    /// </summary>
    Task<IReadOnlyList<Guid>> GetGlobalFallbackPostsAsync(int limit);

    /// <summary>
    /// Get personalized fallback posts based on user's category interests.
    /// Reads from Redis Sorted Set: trending:category:{categoryId}:yyyyMMdd
    /// </summary>
    Task<IReadOnlyList<Guid>> GetPersonalizedFallbackPostsAsync(Guid userId, int limit, CancellationToken ct);

    /// <summary>
    /// Update the global fallback Redis Sorted Set with top posts.
    /// Used by background jobs to maintain stable fallback content.
    /// </summary>
    Task UpdateGlobalFallbackAsync(IReadOnlyList<(Guid PostId, double Score)> posts);
}
