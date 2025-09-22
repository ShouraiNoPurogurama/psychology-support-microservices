using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("user_pinned_posts")]
public class UserPinnedPostsRow
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("pinned_at")]
    public TimeUuid PinnedAt { get; set; }

    [ClusteringKey(1), Column("post_id")]
    public Guid PostId { get; set; }
}
