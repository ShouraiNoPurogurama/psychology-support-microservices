using StackExchange.Redis;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Dtos;
using Feed.Application.MessagePacks;
using Feed.Domain.ViewerFollowing;
using MessagePack;

namespace Feed.Infrastructure.Data.Redis;

public sealed class RankingService(IConnectionMultiplexer redis) : IRankingService
{
    private readonly IDatabase _database = redis.GetDatabase();

    private static readonly MessagePackSerializerOptions MtOpts = MessagePackSerializerOptions.Standard;

    public async Task<IReadOnlyList<Guid>> GetTrendingPostsAsync(DateTimeOffset date, CancellationToken ct)
    {
        var key = GetTrendingKey(date);
        var values = await _database.SortedSetRangeByRankAsync(key, 0, 99, Order.Descending);
        return values
            .Where(v => v.HasValue && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToList();
    }

    public async Task UpdatePostRankAsync(Guid postId, PostRankData rankData)
    {
        var key = GetRankKey(postId);
        var bytes = MessagePackSerializer.Serialize(rankData.ToPack(), MtOpts);

        await _database.StringSetAsync(key, bytes, expiry: TimeSpan.FromDays(7));
    }

    public async Task<IReadOnlyList<RankedPost>> RankPostsAsync(
        IReadOnlyList<Guid> followedAliasIds,
        IReadOnlyList<Guid> trendingPostIds,
        int limit,
        CancellationToken ct)
    {
        var allPostIds = followedAliasIds.Concat(trendingPostIds).Distinct().ToList();
        if (allPostIds.Count == 0) return [];

        var map = await GetPostRanksAsync(allPostIds);
        var tasks = allPostIds.Select(async postId =>
        {
            map.TryGetValue(postId, out var rank);
            var score = rank?.Score ?? 0.0;
            var authorId = await GetPostAuthorAsync(postId, ct) ?? rank?.AuthorAliasId ?? Guid.Empty;

            return new RankedPost(postId,
                authorId,
                (sbyte)Math.Floor(score / 10),
                (long)(score * 1_000_000),
                rank?.CreatedAt ?? DateTimeOffset.UtcNow);
        });
        
        var results = await Task.WhenAll(tasks);
        return results
            .OrderByDescending(p => p.RankI64)
            .Take(limit)
            .ToList();
    }

    public async Task AddToTrendingAsync(Guid postId, double score, DateTimeOffset date, CancellationToken ct)
    {
        var key = GetTrendingKey(date);
        await _database.SortedSetAddAsync(key, postId.ToString(), score);
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(1));
    }

    public async Task InitializePostRankAsync(Guid postId, DateTimeOffset createdAt, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var pack = new PostRankPack
        {
            Score = 0,
            Reactions = 0,
            Comments = 0,
            Ctr = 0.0,
            UpdatedAtSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            CreatedAtSec = createdAt.ToUnixTimeSeconds(),
            AuthorAliasId = Guid.Empty
        };

        var bytes = MessagePackSerializer.Serialize(pack, MtOpts, ct);

        await _database.StringSetAsync(key, bytes, TimeSpan.FromDays(7));
    }

    public async Task<PostRankData?> GetPostRankAsync(Guid postId)
    {
        var key = GetRankKey(postId);
        var val = await _database.StringGetAsync(key, CommandFlags.PreferReplica);
        if (val.IsNullOrEmpty) return null;

        var pack = MessagePackSerializer.Deserialize<PostRankPack>(val, MtOpts);
        return pack.ToPostRankData();
    }

    public async Task<IReadOnlyDictionary<Guid, PostRankData>> GetPostRanksAsync(IReadOnlyList<Guid> postIds)
    {
        if (postIds.Count == 0) return new Dictionary<Guid, PostRankData>();

        var keys = postIds
            .Select(GetRankKey)
            .Select(k => (RedisKey)k)
            .ToArray();

        var values = await _database.StringGetAsync(keys, CommandFlags.PreferReplica);

        var dict = new Dictionary<Guid, PostRankData>();
        for (int i = 0; i < values.Length; i++)
        {
            var val = values[i];
            if (val.IsNullOrEmpty)
                continue;

            var pack = MessagePackSerializer.Deserialize<PostRankPack>(val, MtOpts);

            //Order giống nhau nên có thể lấy thẳng từ index
            dict[postIds[i]] = pack.ToPostRankData();
        }

        return dict;
    }
    
    public async Task IncrementReactionsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var dkey = GetDeltaKey(postId);
        // ghi delta – nhanh, atomic theo field
        await _database.HashIncrementAsync(dkey, "reactions", delta);
        await _database.HashSetAsync(dkey, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(dkey, TimeSpan.FromDays(7));
        // job nền/luồng khác chịu trách nhiệm đọc base pack + delta để tính điểm và StringSet lại key base
    }

    public async Task IncrementCommentsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var dkey = GetDeltaKey(postId);
        await _database.HashIncrementAsync(dkey, "comments", delta);
        await _database.HashSetAsync(dkey, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(dkey, TimeSpan.FromDays(7));
    }

    public async Task SetPostAuthorAsync(Guid postId, Guid authorAliasId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var val = await _database.StringGetAsync(key);
        if(val.IsNullOrEmpty) 
            return;
        
        var pack = MessagePackSerializer.Deserialize<PostRankPack>(val, MtOpts);
        pack.AuthorAliasId = authorAliasId;
        pack.UpdatedAtSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var bytes = MessagePackSerializer.Serialize(pack, MtOpts, ct);
        await _database.StringSetAsync(key, bytes, expiry: TimeSpan.FromDays(7));
    }

    public async Task<Guid?> GetPostAuthorAsync(Guid postId, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        var val = await _database.StringGetAsync(key, CommandFlags.PreferReplica);
        if (val.IsNullOrEmpty) 
            return null;
        
        var pack = MessagePackSerializer.Deserialize<PostRankPack>(val, MtOpts);
        return pack.AuthorAliasId == Guid.Empty ? null : pack.AuthorAliasId;
    }

    public async Task<IReadOnlyList<Guid>> GetGlobalFallbackPostsAsync(int limit)
    {
        const string key = "trending:global_fallback";
        var values = await _database.SortedSetRangeByRankAsync(key, 0, limit - 1, Order.Descending);
        return values
            .Where(v => v.HasValue && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetPersonalizedFallbackPostsAsync(Guid userId, int limit, CancellationToken ct)
    {
        // TODO: Implement personalized fallback logic
        // Logic:
        // 1. Get user's category interests (requires tracking user interactions)
        // 2. For each category, query trending:category:{categoryId}:{date}
        // 3. Merge and deduplicate results
        // 4. Return top N posts

        // For now, return empty list as stub
        await Task.CompletedTask;
        return Array.Empty<Guid>();
    }

    public async Task UpdateGlobalFallbackAsync(IReadOnlyList<(Guid PostId, double Score)> posts)
    {
        const string key = "trending:global_fallback";

        if (posts.Count == 0)
            return;

        var entries = posts.Select(p => new SortedSetEntry(p.PostId.ToString(), p.Score)).ToArray();

        var transaction = _database.CreateTransaction();

        transaction.KeyDeleteAsync(key);

        transaction.SortedSetAddAsync(key, entries);

        transaction.KeyExpireAsync(key, TimeSpan.FromDays(7));

        // Execute transaction as atomic operation
        await transaction.ExecuteAsync();
    }

    private static string GetRankKey(Guid postId) => $"rank:{postId}";
    private static string GetTrendingKey(DateTimeOffset date) => $"trending:{date:yyyyMMdd}";
    private static string GetDeltaKey(Guid postId) => $"rank:delta:{postId}";

}