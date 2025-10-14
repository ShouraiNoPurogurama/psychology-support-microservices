using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Dtos;

namespace Feed.Infrastructure.Data.Repository;

/// <summary>
/// Implementation for IPostReadRepository that delegates to Cassandra post replica repository.
/// This serves as the deepest fallback tier to retrieve posts from Cassandra.
/// </summary>
public sealed class PostReadRepository : IPostReadRepository
{
    private readonly IPostReplicaRepository _postReplicaRepository;

    public PostReadRepository(IPostReplicaRepository postReplicaRepository)
    {
        _postReplicaRepository = postReplicaRepository;
    }

    /// <summary>
    /// Get the most recent public posts from the Cassandra replica tables.
    /// This is a SLOW PATH and should only be used when all Redis caches are empty.
    /// Queries the optimized posts_public_finalized_by_day table.
    /// </summary>
    public async Task<IReadOnlyList<PostInfo>> GetMostRecentPublicPostsAsync(
        int days = 7,
        int limit = 500,
        int startDayOffset = 0,
        CancellationToken cancellationToken = default)
    {
        return await _postReplicaRepository.GetMostRecentPublicPostsAsync(
            days: days,
            limit: limit,
            startDayOffset: startDayOffset,
            ct: cancellationToken);
    }
}
