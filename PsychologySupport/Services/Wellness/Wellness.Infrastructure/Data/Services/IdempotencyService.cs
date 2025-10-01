//using BuildingBlocks.Idempotency;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Wellness.Application.ServiceContracts;
//using Wellness.Domain.Abstractions;
//using Wellness.Domain.Models;

//namespace Wellness.Infrastructure.Data.Services
//{
//    public sealed class IdempotencyService : IIdempotencyService
//    {
//        private readonly IRedisCache _redisCache;
//        private readonly DbContext _dbContext;

//        public IdempotencyService(IRedisCache redisCache, DbContext dbContext)
//        {
//            _redisCache = redisCache;
//            _dbContext = dbContext;
//        }

//        public async Task<bool> RequestExistsAsync(
//            Guid requestKey,
//            CancellationToken cancellationToken = default)
//        {
//            var redisKey = $"idempotency:{requestKey}";

//            // Check Redis
//            var existing = await _redisCache.GetCacheDataAsync<IdempotencyKey>(redisKey);
//            if (existing != null)
//            {
//                return true;
//            }

//            // Check DB
//            var dbKey = await _dbContext.Set<IdempotencyKey>()
//                .FirstOrDefaultAsync(k => k.Key == requestKey, cancellationToken);

//            return dbKey != null;
//        }

//        public async Task<Guid> CreateRequestAsync(
//            Guid requestKey,
//            CancellationToken cancellationToken = default)
//        {
//            var idKey = new IdempotencyKey
//            {
//                Id = Guid.NewGuid(),
//                Key = requestKey,
//                ExpiresAt = DateTimeOffset.UtcNow.AddHours(12),
//                CreatedAt = DateTimeOffset.UtcNow
//            };

//            _dbContext.Add(idKey);
//            await _dbContext.SaveChangesAsync(cancellationToken);

//            var redisKey = $"idempotency:{requestKey}";
//            await _redisCache.SetCacheDataAsync(redisKey, idKey, TimeSpan.FromHours(12));

//            return idKey.Id;
//        }

//        public async Task SaveResponseAsync<T>(
//            Guid requestKey,
//            T response,
//            CancellationToken cancellationToken = default)
//        {
//            var dbKey = await _dbContext.Set<IdempotencyKey>()
//                .FirstOrDefaultAsync(k => k.Key == requestKey, cancellationToken);

//            if (dbKey == null)
//                throw new KeyNotFoundException("Idempotency key not found.");

//            dbKey.ResponsePayload = JsonSerializer.Serialize(response);
//            await _dbContext.SaveChangesAsync(cancellationToken);

//            var redisKey = $"idempotency:{requestKey}";
//            await _redisCache.SetCacheDataAsync(redisKey, dbKey, TimeSpan.FromHours(12));
//        }
//    }
//}
