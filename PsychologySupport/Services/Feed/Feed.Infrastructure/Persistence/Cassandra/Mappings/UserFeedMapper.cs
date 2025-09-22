using Feed.Domain.UserFeed;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserFeedMapper
{
    public static UserFeedByBucketRow ToRow(UserFeedItem domain) => new()
    {
        AliasId = domain.AliasId,
        YmdBucket = CassandraTypeMapper.ToLocalDate(domain.YmdBucket),
        Shard = domain.Shard,
        RankBucket = domain.RankBucket,
        RankI64 = domain.RankI64,
        TsUuid = CassandraTypeMapper.ToTimeUuid(domain.TsUuid),
        PostId = domain.PostId,
        CreatedAt = domain.CreatedAt
    };

    public static UserFeedItem ToDomain(UserFeedByBucketRow row)
        => UserFeedItem.Create(
            row.AliasId,
            row.PostId,
            CassandraTypeMapper.ToDateOnly(row.YmdBucket),
            row.Shard,
            row.RankBucket,
            row.RankI64,
            CassandraTypeMapper.ToGuid(row.TsUuid),
            row.CreatedAt
        );
}
