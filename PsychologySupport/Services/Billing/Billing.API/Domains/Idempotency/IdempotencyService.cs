using Billing.API.Models;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Billing.API.Domains.Idempotency;

public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IDatabase _redis;
    private readonly DbContext _dbContext;

    public IdempotencyService(IConnectionMultiplexer redis, DbContext dbContext)
    {
        _redis = redis.GetDatabase();
        _dbContext = dbContext;
    }

    public async Task<bool> RequestExistsAsync(
        Guid requestKey,
        CancellationToken cancellationToken = default)
    {
        var redisKey = $"idempotency:{requestKey}";

        // Check Redis
        var redisValue = await _redis.StringGetAsync(redisKey);
        if (redisValue.HasValue)
        {
            var existing = JsonSerializer.Deserialize<IdempotencyKey>(redisValue!);
            if (existing != null)
            {
                return true;
            }
        }

        // Check DB
        var dbKey = await _dbContext.Set<IdempotencyKey>()
            .FirstOrDefaultAsync(k => k.Key == requestKey, cancellationToken);

        if (dbKey != null)
        {
            return true;
        }

        return false;
    }

    public async Task<Guid> CreateRequestAsync(
        Guid requestKey,
        CancellationToken cancellationToken = default)
    {
        var idKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = requestKey,
            ExpiresAt = DateTime.UtcNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Add(idKey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var redisKey = $"idempotency:{requestKey}";
        await _redis.StringSetAsync(redisKey, JsonSerializer.Serialize(idKey), TimeSpan.FromHours(12));

        return idKey.Id;
    }

    public async Task SaveResponseAsync<T>(
        Guid requestKey,
        T response,
        CancellationToken cancellationToken = default)
    {
        var dbKey = await _dbContext.Set<IdempotencyKey>()
            .FirstOrDefaultAsync(k => k.Key == requestKey, cancellationToken);

        if (dbKey == null)
            throw new KeyNotFoundException("Idempotency key not found.");

        dbKey.ResponsePayload = JsonSerializer.Serialize(response);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var redisKey = $"idempotency:{requestKey}";
        await _redis.StringSetAsync(redisKey, JsonSerializer.Serialize(dbKey), TimeSpan.FromHours(12));
    }
}
