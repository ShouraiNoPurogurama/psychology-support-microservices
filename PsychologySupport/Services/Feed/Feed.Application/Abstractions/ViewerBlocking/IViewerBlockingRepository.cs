using Feed.Domain.ViewerBlocking;

namespace Feed.Application.Abstractions.ViewerBlocking;

public interface IViewerBlockingRepository
{
    Task<bool> AddIfNotExistsAsync(ViewerBlocked entity, CancellationToken ct);
    Task<bool> RemoveAsync(Guid aliasId, Guid blockedAliasId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid aliasId, Guid blockedAliasId, CancellationToken ct);
    Task<IReadOnlyList<ViewerBlocked>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct);
}
