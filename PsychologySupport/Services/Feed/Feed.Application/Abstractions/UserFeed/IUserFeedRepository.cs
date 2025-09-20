using Cassandra;
using Feed.Domain.UserFeed;

namespace Feed.Application.Abstractions.UserFeed;

public interface IUserFeedRepository
{
    Task<bool> AddFeedItemAsync(UserFeedItem feedItem, CancellationToken ct);
    Task<bool> RemoveFeedItemAsync(Guid aliasId, LocalDate ymdBucket, short shard, Guid postId, CancellationToken ct);
    Task<IReadOnlyList<UserFeedItem>> GetFeedItemsAsync(Guid aliasId, LocalDate ymdBucket, short shard, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<UserFeedItem>> GetUserFeedAsync(Guid aliasId, int days = 7, int limit = 100, CancellationToken ct = default);
}
