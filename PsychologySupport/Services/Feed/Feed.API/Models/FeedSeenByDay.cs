using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.API.Models;

[Table("feed_seen_by_day")]
public class FeedSeenByDay
{
    [PartitionKey(0), Column("AliasId")]
    public Guid AliasId { get; set; }

    [PartitionKey(1), Column("ymd")]
    public LocalDate Ymd { get; set; }

    [ClusteringKey(0), Column("seen_at")]
    public TimeUuid SeenAt { get; set; }

    [ClusteringKey(1), Column("post_id")]
    public Guid PostId { get; set; }
}