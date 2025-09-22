using Billing.Infrastructure.Resilience.Rules;
using BuildingBlocks.Idempotency;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;


namespace Billing.Infrastructure.Resilience.Decorators
{
    public sealed class CachingIdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyService _inner;
        private readonly IConnectionMultiplexer _redis;
        private readonly IdempotencyCacheOptions _opt;

        public CachingIdempotencyService(
            IIdempotencyService inner,
            IConnectionMultiplexer redis,
            IdempotencyCacheOptions opt)
        {
            _inner = inner;
            _redis = redis;
            _opt = opt;
        }

        private static string ExistsKey(Guid k) => IdempotencyRedisKeys.Exists(k);
        private static string RespKey(Guid k) => IdempotencyRedisKeys.Resp(k);

        public async Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken ct = default)
        {
            var db = _redis.GetDatabase();
            if (await db.KeyExistsAsync(ExistsKey(requestKey))) return true;

            var exists = await _inner.RequestExistsAsync(requestKey, ct);
            if (!exists) return false;

            await db.StringSetAsync(ExistsKey(requestKey), "1", _opt.EntryTtl);

            var (found, resp) = await _inner.TryGetResponseAsync<object>(requestKey, ct);
            if (found && resp is not null)
            {
                var json = JsonSerializer.Serialize(resp);
                await db.StringSetAsync(RespKey(requestKey), json, _opt.EntryTtl);
            }
            return true;
        }

        public async Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken ct = default)
        {
            var id = await _inner.CreateRequestAsync(requestKey, ct);
            var db = _redis.GetDatabase();
            await db.StringSetAsync(ExistsKey(requestKey), "1", _opt.EntryTtl);
            return id;
        }

        public async Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(response);

            if (Encoding.UTF8.GetByteCount(payload) > _opt.MaxResponseBytes)
                payload = JsonSerializer.Serialize(new { truncated = true });

            await _inner.SaveResponseAsync(requestKey, response, ct);

            var db = _redis.GetDatabase();
            await db.StringSetAsync(ExistsKey(requestKey), "1", _opt.EntryTtl);
            await db.StringSetAsync(RespKey(requestKey), payload, _opt.EntryTtl);
        }

        public Task<(bool Found, T? Response)> TryGetResponseAsync<T>(Guid requestKey, CancellationToken ct = default)
            => _inner.TryGetResponseAsync<T>(requestKey, ct);
    }
}
