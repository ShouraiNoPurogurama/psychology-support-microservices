using System.Text.Json;
using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore;
using Post.Infrastructure.Data.Public;
using Post.Infrastructure.Resilience.Entities;

namespace Post.Infrastructure.Resilience.Services;

public sealed class IdempotencyService : IIdempotencyService
{
    private readonly PublicDbContext _db;

    private readonly IIdempotencyHashAccessor _hashAccessor;
    private readonly TimeSpan _ttl = TimeSpan.FromHours(12);

    public IdempotencyService(PublicDbContext db, IIdempotencyHashAccessor hashAccessor, Func<string>? getRequestHash = null)
    {
        _db = db;
        _hashAccessor = hashAccessor;
    }

    public async Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken ct = default)
    {
        return await _db.IdempotencyKeys
            .AsNoTracking()
            .AnyAsync(k => k.Key == requestKey, ct);
    }

    public async Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken ct = default)
    {
        var requestHash = _hashAccessor.RequestHash ?? "unknown";

        var existing = await _db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == requestKey, ct);

        if (existing is not null && !string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            throw new InvalidOperationException("Idempotency-Key reused with different request payload.");

        if (existing is not null) return existing.Id;

        var entity = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = requestKey,
            RequestHash = requestHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(_ttl)
        };
        _db.IdempotencyKeys.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken ct = default)
    {
        var entity = await _db.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == requestKey, ct);

        if (entity is null)
            throw new KeyNotFoundException($"Idempotency key {requestKey} not found.");

        entity.ResponsePayload = JsonSerializer.Serialize(response);
        entity.ExpiresAt = DateTimeOffset.UtcNow.Add(_ttl);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(bool Found, T? Response)> TryGetResponseAsync<T>(Guid requestKey, CancellationToken ct = default)
    {
        var entity = await _db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == requestKey, ct);

        if (entity?.ResponsePayload is null) return (false, default);
        return (true, JsonSerializer.Deserialize<T>(entity.ResponsePayload)!);
    }
}
