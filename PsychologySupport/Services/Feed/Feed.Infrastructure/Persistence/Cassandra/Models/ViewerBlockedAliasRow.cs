using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("viewer_blocked_alias")]
public class ViewerBlockedAliasRow
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("blocked_alias_id")]
    public Guid BlockedAliasId { get; set; }

    [Column("blocked_since")]
    public DateTimeOffset? BlockedSince { get; set; }
}
