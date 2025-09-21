namespace Feed.Domain.IdempotencyKey;

/// <summary>
/// Represents an idempotent request within the domain.
/// It's persistence-agnostic and enforces its own invariants.
/// </summary>
public class IdempotencyKey
{
    /// <summary>
    /// The idempotency key provided by the client.
    /// </summary>
    public Guid Key { get; }

    /// <summary>
    /// A hash of the request payload to prevent key reuse with different data.
    /// </summary>
    public string RequestHash { get; }

    /// <summary>
    /// The serialized response payload to return on subsequent requests.
    /// </summary>
    public string? ResponsePayload { get; private set; }

    /// <summary>
    /// The absolute expiration time for this key.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>
    /// The creation timestamp of the initial request.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// The alias ID of the user who initiated the request.
    /// </summary>
    public Guid CreatedByAliasId { get; }

    private IdempotencyKey(
        Guid key,
        string requestHash,
        string? responsePayload,
        DateTimeOffset? expiresAt,
        DateTimeOffset createdAt,
        Guid createdByAliasId)
    {
        // Enforce invariants (business rules)
        if (key == Guid.Empty)
            throw new ArgumentException("Idempotency key cannot be empty.", nameof(key));
        if (string.IsNullOrWhiteSpace(requestHash))
            throw new ArgumentException("Request hash cannot be empty.", nameof(requestHash));
        if (createdByAliasId == Guid.Empty)
            throw new ArgumentException("Creator alias ID cannot be empty.", nameof(createdByAliasId));
            
        Key = key;
        RequestHash = requestHash;
        ResponsePayload = responsePayload;
        ExpiresAt = expiresAt?.ToUniversalTime();
        CreatedAt = createdAt.ToUniversalTime();
        CreatedByAliasId = createdByAliasId;
    }

    /// <summary>
    /// Factory method to create a new IdempotencyRequest instance.
    /// </summary>
    public static IdempotencyKey Create(
        Guid key,
        string requestHash,
        Guid createdByAliasId,
        DateTimeOffset? expiresAt,
        string? responsePayload = null)
    {
        return new IdempotencyKey(
            key,
            requestHash,
            responsePayload,
            expiresAt,
            DateTimeOffset.UtcNow,
            createdByAliasId
        );
    }
    
    /// <summary>
    /// Factory method to reconstitute an IdempotencyRequest from persistence.
    /// </summary>
    public static IdempotencyKey FromPersistence(
        Guid key,
        string requestHash,
        string? responsePayload,
        DateTimeOffset? expiresAt,
        DateTimeOffset createdAt,
        Guid createdByAliasId)
    {
        return new IdempotencyKey(key, requestHash, responsePayload, expiresAt, createdAt, createdByAliasId);
    }

    /// <summary>
    /// Sets the response payload after the operation has completed.
    /// </summary>
    public void SetResponse(string payload)
    {
        ResponsePayload = payload;
    }
}