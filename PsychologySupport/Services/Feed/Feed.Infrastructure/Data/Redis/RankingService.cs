using StackExchange.Redis;
using System.Text.Json;
using Feed.Application.Features.UserFeed.Queries.GetFeed;

namespace Feed.Infrastructure.Data.Redis;

public interface IRankingService
{
    Task<IReadOnlyList<Guid>> GetTrendingPostsAsync(DateTime date, CancellationToken ct);
    Task UpdatePostRankAsync(Guid postId, PostRankData rankData, CancellationToken ct);
    Task<IReadOnlyList<RankedPost>> RankPostsAsync(IReadOnlyList<Guid> followedAliasIds, IReadOnlyList<Guid> trendingPostIds, int limit, CancellationToken ct);
    Task AddToTrendingAsync(Guid postId, double score, DateTime date, CancellationToken ct);
}

public record PostRankData(
    double Score,
    int Reactions,
    int Comments,
    double Ctr,
    DateTimeOffset UpdatedAt
);

public sealed class RankingService(IConnectionMultiplexer redis) : IRankingService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<IReadOnlyList<Guid>> GetTrendingPostsAsync(DateTime date, CancellationToken ct)
    {
        var key = GetTrendingKey(date);
        var values = await _database.SortedSetRangeByRankAsync(key, 0, 99, Order.Descending);
        
        return values
            .Where(v => v.HasValue && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToList();
    }

    public async Task UpdatePostRankAsync(Guid postId, PostRankData rankData, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var hash = new HashEntry[]
        {
            new("score", rankData.Score),
            new("reactions", rankData.Reactions),
            new("comments", rankData.Comments),
            new("ctr", rankData.Ctr),
            new("updated_at", rankData.UpdatedAt.ToUnixTimeSeconds())
        };

        await _database.HashSetAsync(key, hash);
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7)); // TTL for rank cache
    }

    public async Task<IReadOnlyList<RankedPost>> RankPostsAsync(
        IReadOnlyList<Guid> followedAliasIds, 
        IReadOnlyList<Guid> trendingPostIds, 
        int limit, 
        CancellationToken ct)
    {
        var allPostIds = followedAliasIds.Concat(trendingPostIds).Distinct().ToList();
        var rankedPosts = new List<RankedPost>();

        // Get rank data for all posts
        var tasks = allPostIds.Select(async postId =>
        {
            var rankData = await GetPostRankDataAsync(postId, ct);
            return new RankedPost(
                postId,
                Guid.Empty, // Would need to fetch from Post service
                (sbyte)Math.Floor(rankData?.Score / 10 ?? 0),
                (long)((rankData?.Score ?? 0) * 1_000_000),
                DateTime.UtcNow // Would need actual creation date
            );
        });

        var results = await Task.WhenAll(tasks);
        
        return results
            .OrderByDescending(p => p.RankI64)
            .Take(limit)
            .ToList();
    }

    public async Task AddToTrendingAsync(Guid postId, double score, DateTime date, CancellationToken ct)
    {
        var key = GetTrendingKey(date);
        await _database.SortedSetAddAsync(key, postId.ToString(), score);
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(1)); // TTL for trending
    }

    private async Task<PostRankData?> GetPostRankDataAsync(Guid postId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var hash = await _database.HashGetAllAsync(key);
        
        if (hash.Length == 0) return null;

        var hashDict = hash.ToDictionary(h => h.Name, h => h.Value);

        return new PostRankData(
            hashDict.TryGetValue("score", out var score) ? (double)score : 0.0,
            hashDict.TryGetValue("reactions", out var reactions) ? (int)reactions : 0,
            hashDict.TryGetValue("comments", out var comments) ? (int)comments : 0,
            hashDict.TryGetValue("ctr", out var ctr) ? (double)ctr : 0.0,
            hashDict.TryGetValue("updated_at", out var updatedAt) 
                ? DateTimeOffset.FromUnixTimeSeconds((long)updatedAt)
                : DateTimeOffset.UtcNow
        );
    }

    private static string GetRankKey(Guid postId) => $"rank:{postId}";
    private static string GetTrendingKey(DateTime date) => $"trending:{date:yyyyMMdd}";
}
