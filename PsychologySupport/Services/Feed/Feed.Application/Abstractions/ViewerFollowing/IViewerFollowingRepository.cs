using Feed.Domain.ViewerFollowing;

namespace Feed.Application.Abstractions.ViewerFollowing;

public interface IViewerFollowingRepository
{
    Task<bool> AddIfNotExistsAsync(ViewerFollow entity, CancellationToken ct);
    Task<bool> RemoveAsync(Guid aliasId, Guid followedAliasId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid aliasId, Guid followedAliasId, CancellationToken ct);
    Task<IReadOnlyList<ViewerFollow>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct);
}
