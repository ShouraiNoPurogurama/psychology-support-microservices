using System.Text.Json;
using Billing.API.Models;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Billing.API.Domains.Idempotency;

public class IdempotencyService
{
    private readonly IDatabase _redis;

    public IdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<(T? Result, IdempotencyKey IdempotencyKey)> CheckOrCreateAsync<T, TDbContext>(
        TDbContext dbContext,
        Guid idempotencyKey,
        object requestDto,
        Func<IdempotencyKey, Task<T>> createFunc,
        CancellationToken cancellationToken
    ) where TDbContext : DbContext
    {
        var requestJson = JsonSerializer.Serialize(requestDto);
        var requestHash = ComputeSha256(requestJson);

        // Redis lookup for key existence and expiration
        var redisKey = $"idempotency:{idempotencyKey}";
        var redisValue = await _redis.StringGetAsync(redisKey);
        if (redisValue.HasValue)
        {
            var existing = JsonSerializer.Deserialize<IdempotencyKey>(redisValue!);
            if (existing != null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Idempotency key already used with different request payload.");

                if (!string.IsNullOrEmpty(existing.ResponsePayload))
                {
                    var replay = JsonSerializer.Deserialize<T>(existing.ResponsePayload);
                    if (replay != null) return (replay, existing);
                }

                throw new ConflictException("Request with same idempotency key is in progress.");
            }
        }

        // Fallback to DB lookup if not in Redis
        var existingKey = await dbContext.Set<IdempotencyKey>()
            .FirstOrDefaultAsync(k => k.Key == idempotencyKey, cancellationToken);

        if (existingKey != null)
        {
            if (!string.Equals(existingKey.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Idempotency key already used with different request payload.");

            if (!string.IsNullOrEmpty(existingKey.ResponsePayload))
            {
                var replay = JsonSerializer.Deserialize<T>(existingKey.ResponsePayload);
                if (replay != null) return (replay, existingKey);
            }

            throw new ConflictException("Request with same idempotency key is in progress.");
        }

        // Create new key
        var idKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = idempotencyKey,
            RequestHash = requestHash,
            ExpiresAt = DateTime.UtcNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Add(idKey);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Store in Redis with expiration
        await _redis.StringSetAsync(redisKey, JsonSerializer.Serialize(idKey), TimeSpan.FromHours(12));

        // Proceed with creation
        var result = await createFunc(idKey);

        // Save response payload for replay
        idKey.ResponsePayload = JsonSerializer.Serialize(result);
        await dbContext.SaveChangesAsync(cancellationToken);
        await _redis.StringSetAsync(redisKey, JsonSerializer.Serialize(idKey), TimeSpan.FromHours(12));

        return (result, idKey);
    }

    private static string ComputeSha256(string rawData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
