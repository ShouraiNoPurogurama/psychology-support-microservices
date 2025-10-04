using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

/// <summary>
/// Cassandra row model for posts_replica table.
/// General post data replication with day-based partitioning.
/// </summary>
[Table("posts_replica")]
public class PostReplicaRow
{
    [PartitionKey, Column("ymd_bucket")]
    public LocalDate YmdBucket { get; set; }

    [ClusteringKey(0), Column("created_at")]
    public TimeUuid CreatedAt { get; set; }

    [ClusteringKey(1), Column("post_id")]
    public Guid PostId { get; set; }

    [Column("author_alias_id")]
    public Guid AuthorAliasId { get; set; }

    [Column("visibility")]
    public string Visibility { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;
}
