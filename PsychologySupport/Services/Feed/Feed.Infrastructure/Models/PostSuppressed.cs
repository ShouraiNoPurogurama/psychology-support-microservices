namespace Feed.Infrastructure.Models;

[Table("post_suppressed")]
public class PostSuppressed
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