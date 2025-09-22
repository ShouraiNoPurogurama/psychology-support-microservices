using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("follows_by_viewer")]
public class FollowsByViewerRow
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("followed_alias_id")]
    public Guid FollowedAliasId { get; set; }

    [Column("since")]
    public DateTimeOffset? Since { get; set; }
}
