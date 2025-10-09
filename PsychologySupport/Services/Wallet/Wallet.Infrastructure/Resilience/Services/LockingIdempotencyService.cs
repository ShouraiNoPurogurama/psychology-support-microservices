using BuildingBlocks.Idempotency;
using StackExchange.Redis;
using Wallet.Infrastructure.Resilience.Rules;

namespace  Wallet.Infrastructure.Resilience.Services
{
    public sealed class LockingIdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyService _inner;
        private readonly IConnectionMultiplexer _redis;
        private readonly IdempotencyCacheOptions _opt;

        public LockingIdempotencyService(
            IIdempotencyService inner,
            IConnectionMultiplexer redis,
            IdempotencyCacheOptions opt)
        {
            _inner = inner;
            _redis = redis;
            _opt = opt;
        }

        public Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken cancellationToken = default)
            => _inner.RequestExistsAsync(requestKey, cancellationToken);

        public async Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken cancellationToken = default)
        {
            var db = _redis.GetDatabase();
            var lockKey = IdempotencyRedisKeys.Lock(requestKey);

            var acquired = await db.StringSetAsync(lockKey, Environment.MachineName, _opt.LockTtl, When.NotExists);
            if (!acquired) throw new InvalidOperationException("Idempotent request is being processed.");

            try { return await _inner.CreateRequestAsync(requestKey, cancellationToken); }
            finally { _ = db.KeyDeleteAsync(lockKey); } //best-effort (TTL bảo hiểm crash)
        }

        public Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken cancellationToken = default)
            => _inner.SaveResponseAsync(requestKey, response, cancellationToken);

        public Task<(bool Found, T? Response)> TryGetResponseAsync<T>(Guid requestKey, CancellationToken cancellationToken = default)
            => _inner.TryGetResponseAsync<T>(requestKey, cancellationToken);
    }
}
