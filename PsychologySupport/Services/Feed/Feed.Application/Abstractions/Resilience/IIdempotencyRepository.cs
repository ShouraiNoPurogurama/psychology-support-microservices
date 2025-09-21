using Feed.Domain.IdempotencyKey;

namespace Feed.Application.Abstractions.Resilience;

public interface IIdempotencyRepository
{
    /// <summary>
    /// Gets an idempotency request by its key.
    /// </summary>
    Task<IdempotencyKey?> GetAsync(Guid key, CancellationToken ct = default);

    /// <summary>
    /// Attempts to create a new idempotency request placeholder.
    /// This operation is atomic.
    /// </summary>
    /// <returns>True if the request was created; false if a request with the same key already existed.</returns>
    Task<bool> TryCreateAsync(IdempotencyKey request, TimeSpan ttl, CancellationToken ct = default);
    
    /// <summary>
    /// Updates an existing idempotency request, typically to add the response payload.
    /// </summary>
    Task UpdateAsync(IdempotencyKey request, TimeSpan ttl, CancellationToken ct = default);
}