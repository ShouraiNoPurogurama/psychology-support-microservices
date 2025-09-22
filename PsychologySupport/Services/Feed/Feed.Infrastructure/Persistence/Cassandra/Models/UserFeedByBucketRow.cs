using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("user_feed_by_bucket")]
public class UserFeedByBucketRow
{
    [PartitionKey(0), Column("alias_id")]
    public Guid AliasId { get; set; }

    [PartitionKey(1), Column("ymd_bucket")]
    public LocalDate YmdBucket { get; set; }

    [PartitionKey(2), Column("shard")]
    public short Shard { get; set; }

    [ClusteringKey(0), Column("rank_bucket")]
    public sbyte RankBucket { get; set; }

    [ClusteringKey(1), Column("rank_i64")]
    public long RankI64 { get; set; }

    [ClusteringKey(2), Column("ts_uuid")]
    public TimeUuid TsUuid { get; set; }

    [ClusteringKey(3), Column("post_id")]
    public Guid PostId { get; set; }

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}
