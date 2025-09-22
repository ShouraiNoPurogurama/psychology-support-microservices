using Feed.Domain.FeedConfiguration;

namespace Feed.Application.Abstractions.FeedConfiguration;

public interface IFeedConfigRepository
{
    Task<bool> SetAsync(FeedConfig config, CancellationToken ct);
    Task<FeedConfig?> GetAsync(string key, CancellationToken ct);
    Task<bool> RemoveAsync(string key, CancellationToken ct);
    Task<IReadOnlyList<FeedConfig>> GetAllAsync(CancellationToken ct);
}
