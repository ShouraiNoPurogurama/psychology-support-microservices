namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("followers_of_alias")]
public class FollowersOfAliasRow
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }
    
    [ClusteringKey, Column("follower_alias_id")]
    public Guid FollowerAliasId { get; set; }

    [Column("since")]
    public DateTimeOffset? Since { get; set; }
}
