using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("post_suppressed")]
public class PostSuppressedRow
{
    [PartitionKey, Column("post_id")]
    public Guid PostId { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("suppressed_at")]
    public DateTimeOffset? SuppressedAt { get; set; }

    [Column("suppressed_until")]
    public DateTimeOffset? SuppressedUntil { get; set; }
}
