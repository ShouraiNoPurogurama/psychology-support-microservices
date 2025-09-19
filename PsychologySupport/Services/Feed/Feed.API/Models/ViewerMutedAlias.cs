using Cassandra.Mapping.Attributes;

namespace Feed.API.Models;

[Table("viewer_muted_alias")]
public class ViewerMutedAlias
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("muted_alias_id")]
    public Guid MutedAliasId { get; set; }

    [Column("muted_since")]
    public DateTimeOffset? MutedSince { get; set; }
}