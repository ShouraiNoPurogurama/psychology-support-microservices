using Feed.Domain.UserFeed;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserFeedMapper
{
    public static UserFeedByBucketRow ToRow(UserFeedItem domain) => new()
    {
        AliasId = domain.AliasId,
        YmdBucket = domain.YmdBucket,
        Shard = domain.Shard,
        RankBucket = domain.RankBucket,
        RankI64 = domain.RankI64,
        TsUuid = domain.TsUuid,
        PostId = domain.PostId,
        CreatedAt = domain.CreatedAt
    };

    public static UserFeedItem ToDomain(UserFeedByBucketRow row)
        => UserFeedItem.Create(
            row.AliasId,
            row.PostId,
            row.YmdBucket,
            row.Shard,
            row.RankBucket,
            row.RankI64,
            row.TsUuid,
            row.CreatedAt
        );
}
