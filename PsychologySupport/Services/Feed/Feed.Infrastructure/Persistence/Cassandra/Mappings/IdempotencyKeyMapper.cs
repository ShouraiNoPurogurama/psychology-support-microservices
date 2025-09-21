using Feed.Domain.IdempotencyKey;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class IdempotencyKeyMapper
{
    /// <summary>
    /// Maps a domain entity to a Cassandra row DTO.
    /// </summary>
    public static IdempotencyKeyRow ToRow(IdempotencyKey domain) => new()
    {
        Key = domain.Key,
        RequestHash = domain.RequestHash,
        ResponsePayload = domain.ResponsePayload,
        ExpiresAt = domain.ExpiresAt,
        CreatedAt = domain.CreatedAt,
        CreatedByAliasId = domain.CreatedByAliasId
    };

    /// <summary>
    /// Maps a Cassandra row DTO back to a domain entity, ensuring domain rules are applied.
    /// </summary>
    public static IdempotencyKey ToDomain(IdempotencyKeyRow row)
        => IdempotencyKey.FromPersistence(
            row.Key,
            row.RequestHash,
            row.ResponsePayload,
            row.ExpiresAt,
            // Provide a fallback if CreatedAt is somehow null in the DB
            row.CreatedAt ?? DateTimeOffset.MinValue, 
            row.CreatedByAliasId
        );
}