namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("followers_by_alias")]
public class FollowersByAliasRow
{
    [PartitionKey, Column("follower_alias_id")]
    public Guid FollowerAliasId { get; set; }

    [ClusteringKey(0), Column("alias_id")]
    public Guid AliasId { get; set; }

    [Column("since")]
    public DateTimeOffset? Since { get; set; }
}
