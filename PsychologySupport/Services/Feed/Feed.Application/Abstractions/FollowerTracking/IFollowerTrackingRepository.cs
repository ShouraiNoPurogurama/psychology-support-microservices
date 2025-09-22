using Feed.Domain.FollowerTracking;

namespace Feed.Application.Abstractions.FollowerTracking;

public interface IFollowerTrackingRepository
{
    Task<bool> AddIfNotExistsAsync(Follower entity, CancellationToken ct);
    Task<bool> RemoveAsync(Guid authorAliasId, Guid aliasId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid authorAliasId, Guid aliasId, CancellationToken ct);
    Task<IReadOnlyList<Follower>> GetAllFollowersAsync(Guid authorAliasId, CancellationToken ct);
}
