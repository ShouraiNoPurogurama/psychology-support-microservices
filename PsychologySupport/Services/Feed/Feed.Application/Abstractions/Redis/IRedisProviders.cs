
namespace Feed.Application.Abstractions.Redis;

public interface ITrendingProvider
{
    Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct);
    Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct);
}