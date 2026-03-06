using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

/// <summary>
/// Cassandra row model for PostEngagementMetadata table.
/// Stores author information and timestamp tracking for engagement updates.
/// </summary>
[Table("PostEngagementMetadata")]
public class PostEngagementMetadataRow
{
    [PartitionKey, Column("post_id")]
    public Guid PostId { get; set; }

    [Column("author_alias_id")]
    public Guid AuthorAliasId { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("counters_last_updated")]
    public DateTimeOffset CountersLastUpdated { get; set; }
}
