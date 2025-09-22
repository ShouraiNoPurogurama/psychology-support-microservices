using StackExchange.Redis;
using Feed.Application.Abstractions.Redis;
using Feed.Infrastructure.Data.Redis;
using System.Text.Json;

namespace Feed.Infrastructure.Data.Redis.Providers;

internal sealed class TrendingRedisProvider : ITrendingProvider
{
    private readonly IDatabase _database;

    public TrendingRedisProvider(IDatabase database)
    {
        _database = database;
    }

    public async Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        var key = RedisKeyPatterns.TrendingDaily(date);
        await _database.SortedSetAddAsync(key, postId.ToString(), score);
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(1)); // TTL for trending
    }

    public async Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct)
    {
        var key = RedisKeyPatterns.TrendingDaily(date);
        var values = await _database.SortedSetRangeByRankAsync(key, 0, count - 1, Order.Descending);
        
        return values
            .Where(v => v.HasValue && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToList();
    }

    public async Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        var key = RedisKeyPatterns.TrendingDaily(date);
        await _database.SortedSetAddAsync(key, postId.ToString(), score);
    }
}