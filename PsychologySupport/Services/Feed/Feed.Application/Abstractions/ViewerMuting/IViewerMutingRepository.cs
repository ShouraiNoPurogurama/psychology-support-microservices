using Feed.Domain.ViewerMuting;

namespace Feed.Application.Abstractions.ViewerMuting;

public interface IViewerMutingRepository
{
    Task<bool> AddIfNotExistsAsync(ViewerMuted entity, CancellationToken ct);
    Task<bool> RemoveAsync(Guid aliasId, Guid mutedAliasId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid aliasId, Guid mutedAliasId, CancellationToken ct);
    Task<IReadOnlyList<ViewerMuted>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct);
}
