using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.API.Models;

[Table("user_pinned_posts")]
public class UserPinnedPosts
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("pinned_at")]
    public TimeUuid PinnedAt { get; set; }

    [ClusteringKey(1), Column("post_id")]
    public Guid PostId { get; set; }
}