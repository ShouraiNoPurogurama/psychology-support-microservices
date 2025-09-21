namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("idempotency_keys")]
public class IdempotencyKeyRow
{
    /// <summary>
    /// The idempotency key from the request header. This is the sole partition key
    /// for efficient key-value lookups.
    /// </summary>
    [PartitionKey, Column("key")]
    public Guid Key { get; set; }

    [Column("request_hash")]
    public string RequestHash { get; set; } = null!;

    [Column("response_payload")]
    public string? ResponsePayload { get; set; }

    [Column("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("created_by_alias_id")]
    public Guid CreatedByAliasId { get; set; }
}