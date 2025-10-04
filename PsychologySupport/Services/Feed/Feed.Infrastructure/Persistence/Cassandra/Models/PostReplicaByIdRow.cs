using Cassandra;
using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

/// <summary>
/// Cassandra row model for post_replica_by_id table.
/// Lookup table mapping post IDs to their partition keys.
/// Enables efficient delete and update operations.
/// </summary>
[Table("post_replica_by_id")]
public class PostReplicaByIdRow
{
    [PartitionKey, Column("post_id")]
    public Guid PostId { get; set; }

    [Column("ymd_bucket")]
    public LocalDate YmdBucket { get; set; }

    [Column("created_at")]
    public TimeUuid CreatedAt { get; set; }
}
