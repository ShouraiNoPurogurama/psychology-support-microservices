namespace Feed.Infrastructure.Models;

[Table("followers_by_alias")]
public class FollowersByAlias
{
    [PartitionKey, Column("author_alias_id")]
    public Guid AuthorAliasId { get; set; }

    [ClusteringKey(0), Column("alias_id")]
    public Guid AliasId { get; set; }

    [Column("since")]
    public DateTimeOffset? Since { get; set; }
}