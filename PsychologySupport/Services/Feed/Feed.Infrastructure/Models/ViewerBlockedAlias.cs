namespace Feed.Infrastructure.Models;

[Table("viewer_blocked_alias")]
public class ViewerBlockedAlias
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("blocked_alias_id")]
    public Guid BlockedAliasId { get; set; }

    [Column("blocked_since")]
    public DateTimeOffset? BlockedSince { get; set; }
}