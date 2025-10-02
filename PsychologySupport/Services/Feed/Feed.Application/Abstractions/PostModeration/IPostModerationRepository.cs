using Feed.Domain.PostModeration;

namespace Feed.Application.Abstractions.PostModeration;

public interface IPostModerationRepository
{
    Task<bool> SuppressAsync(PostSuppression suppression, CancellationToken ct);
    Task<bool> UnsuppressAsync(Guid postId, CancellationToken ct);
    Task<PostSuppression?> GetSuppressionAsync(Guid postId, CancellationToken ct);
    Task<bool> IsCurrentlySuppressedAsync(Guid postId, CancellationToken ct);
    
    // Batch operations for performance optimization
    Task<IReadOnlyDictionary<Guid, PostSuppression?>> GetSuppressionsBatchAsync(IReadOnlyList<Guid> postIds, CancellationToken ct);
    Task<IReadOnlySet<Guid>> GetSuppressedPostIdsBatchAsync(IReadOnlyList<Guid> postIds, CancellationToken ct);
}
