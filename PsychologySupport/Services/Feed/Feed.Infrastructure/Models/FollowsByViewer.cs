namespace Feed.Infrastructure.Models;

[Table("follows_by_viewer")]
public class FollowsByViewer
{
    [PartitionKey, Column("alias_id")]
    public Guid AliasId { get; set; }

    [ClusteringKey(0), Column("followed_alias_id")]
    public Guid FollowedAliasId { get; set; }

    [Column("since")]
    public DateTimeOffset? Since { get; set; }
}