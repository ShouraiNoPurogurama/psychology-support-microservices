using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

/// <summary>
/// Cassandra row model for posts_public_finalized_by_day table.
/// Pre-filtered query table containing only public, finalized posts.
/// Optimized for fast retrieval without ALLOW FILTERING.
/// </summary>
[Table("posts_public_finalized_by_day")]
public class PostPublicFinalizedByDayRow
{
    [PartitionKey, Column("ymd_bucket")]
    public LocalDate YmdBucket { get; set; }

    [ClusteringKey(0), Column("created_at")]
    public TimeUuid CreatedAt { get; set; }

    [ClusteringKey(1), Column("post_id")]
    public Guid PostId { get; set; }

    [Column("author_alias_id")]
    public Guid AuthorAliasId { get; set; }
}
