using StackExchange.Redis;
using Feed.Application.Abstractions.RankingService;

namespace Feed.Infrastructure.Data.Redis;

public sealed class RankingService(IConnectionMultiplexer redis) : IRankingService
{
    private readonly IDatabase _database = redis.GetDatabase();

    private static class RankFields
    {
        public const string Score = "score";
        public const string Reactions = "reactions";
        public const string Comments = "comments";
        public const string Ctr = "ctr";
        public const string UpdatedAt = "updated_at";
        public const string CreatedAt = "created_at";
        public const string AuthorId = "author_id";
    }

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
            new(RankFields.Score, rankData.Score),
            new(RankFields.Reactions, rankData.Reactions),
            new(RankFields.Comments, rankData.Comments),
            new(RankFields.Ctr, rankData.Ctr),
            new(RankFields.UpdatedAt, rankData.UpdatedAt.ToUnixTimeSeconds()),
            new(RankFields.CreatedAt, rankData.CreatedAt.ToUnixTimeSeconds()),
        };
        await _database.HashSetAsync(key, hash);
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task<IReadOnlyList<RankedPost>> RankPostsAsync(
        IReadOnlyList<Guid> followedAliasIds,
        IReadOnlyList<Guid> trendingPostIds,
        int limit,
        CancellationToken ct)
    {
        var allPostIds = followedAliasIds.Concat(trendingPostIds).Distinct().ToList();
        var tasks = allPostIds.Select(async postId =>
        {
            var rank = await GetPostRankAsync(postId, ct);
            var score = rank?.Score ?? 0;
            var authorId = await GetPostAuthorAsync(postId, ct) ?? Guid.Empty;
            return new RankedPost(
                postId,
                authorId,
                (sbyte)Math.Floor(score / 10),
                (long)(score * 1_000_000),
                rank?.CreatedAt ?? DateTimeOffset.UtcNow
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
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(1));
    }

    public async Task InitializePostRankAsync(Guid postId, DateTimeOffset createdAt, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashSetAsync(key, new[]
        {
            new HashEntry(RankFields.CreatedAt, createdAt.ToUnixTimeSeconds()),
            new HashEntry(RankFields.Score, 0),
            new HashEntry(RankFields.Reactions, 0),
            new HashEntry(RankFields.Comments, 0),
            new HashEntry(RankFields.Ctr, 0.0),
            new HashEntry(RankFields.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task<PostRankData?> GetPostRankAsync(Guid postId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var hash = await _database.HashGetAllAsync(key);
        if (hash.Length == 0) return null;
        var dict = hash.ToDictionary(h => h.Name, h => h.Value);

        Guid? authorId = null;
        if (dict.TryGetValue(RankFields.AuthorId, out var authorVal) && Guid.TryParse(authorVal.ToString(), out var parsed))
            authorId = parsed;

        return new PostRankData(
            dict.TryGetValue(RankFields.Score, out var score) ? (double)score : 0.0,
            dict.TryGetValue(RankFields.Reactions, out var reactions) ? (int)reactions : 0,
            dict.TryGetValue(RankFields.Comments, out var comments) ? (int)comments : 0,
            dict.TryGetValue(RankFields.Ctr, out var ctr) ? (double)ctr : 0.0,
            dict.TryGetValue(RankFields.UpdatedAt, out var updatedAt) ? DateTimeOffset.FromUnixTimeSeconds((long)updatedAt) : DateTimeOffset.UtcNow,
            dict.TryGetValue(RankFields.CreatedAt, out var createdAt) ? DateTimeOffset.FromUnixTimeSeconds((long)createdAt) : DateTimeOffset.UtcNow,
            authorId ?? Guid.Empty
        );
    }

    private async Task<PostRankData?> GetPostRankDataAsync(Guid postId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var hash = await _database.HashGetAllAsync(key);

        if (hash.Length == 0) return null;

        var hashDict = hash.ToDictionary(h => h.Name, h => h.Value);

        return new PostRankData(
            hashDict.TryGetValue(RankFields.Score, out var score) ? (double)score : 0.0,
            hashDict.TryGetValue(RankFields.Reactions, out var reactions) ? (int)reactions : 0,
            hashDict.TryGetValue(RankFields.Comments, out var comments) ? (int)comments : 0,
            hashDict.TryGetValue(RankFields.Ctr, out var ctr) ? (double)ctr : 0.0,
            hashDict.TryGetValue(RankFields.UpdatedAt, out var updatedAt)
                ? DateTimeOffset.FromUnixTimeSeconds((long)updatedAt)
                : DateTimeOffset.UtcNow,
            hashDict.TryGetValue(RankFields.CreatedAt, out var createdAt)
                ? DateTimeOffset.FromUnixTimeSeconds((long)createdAt)  
                : DateTimeOffset.UtcNow,
            hashDict.TryGetValue(RankFields.AuthorId, out var authorId) && Guid.TryParse(authorId, out var parsedAuthorId)
                ? parsedAuthorId
                : Guid.Empty
        );
    }

    public async Task IncrementReactionsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, RankFields.Reactions, delta);
        await _database.HashSetAsync(key, new[] { new HashEntry(RankFields.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task IncrementCommentsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, RankFields.Comments, delta);
        await _database.HashSetAsync(key, new[] { new HashEntry(RankFields.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task SetPostAuthorAsync(Guid postId, Guid authorAliasId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashSetAsync(key, new[] { new HashEntry(RankFields.AuthorId, authorAliasId.ToString()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task<Guid?> GetPostAuthorAsync(Guid postId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var value = await _database.HashGetAsync(key, RankFields.AuthorId);
        if (value.HasValue && Guid.TryParse(value.ToString(), out var gid)) return gid;
        return null;
    }

    private static string GetRankKey(Guid postId) => $"rank:{postId}";
    private static string GetTrendingKey(DateTime date) => $"trending:{date:yyyyMMdd}";
}
