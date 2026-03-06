using Feed.Application.Abstractions.RankingService;
using Feed.Application.Dtos;
using Feed.Application.MessagePacks;
using MessagePack;
using StackExchange.Redis;

namespace Feed.Application.Services;

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

    // Trong RankingService.cs
    public async Task InitializePostRankAsync(Guid postId, DateTimeOffset createdAt, Guid authorAliasId, CancellationToken ct)
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
            AuthorAliasId = authorAliasId,
            Shares = 0,
            Clicks = 0,
            Impressions = 0,
            ViewDurationSec = 0.0
        };

        var bytes = MessagePackSerializer.Serialize(pack, MtOpts, ct);

        await _database.StringSetAsync(key, bytes, TimeSpan.FromDays(7), When.NotExists);
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
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, "reactions", delta);
        await _database.HashSetAsync(key, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task IncrementCommentsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, "comments", delta);
        await _database.HashSetAsync(key, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task IncrementClicksAsync(Guid postId, int delta, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, "clicks", delta);
        await _database.HashSetAsync(key, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task IncrementImpressionsAsync(Guid postId, int delta, CancellationToken ct)
    {
        var key = GetRankKey(postId);
        await _database.HashIncrementAsync(key, "impressions", delta);
        await _database.HashSetAsync(key, "updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds());       
        await _database.KeyExpireAsync(key, TimeSpan.FromDays(7));
    }

    public async Task IncrementViewDurationAsync(Guid postId, int delta, CancellationToken ct)
    {
        var dkey = GetRankKey(postId);
        await _database.HashIncrementAsync(dkey, "view_duration", delta);
        await _database.HashSetAsync(dkey, new[] { new HashEntry("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) });
        await _database.KeyExpireAsync(dkey, TimeSpan.FromDays(7));
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
        string key = GetTrendingGlobalFallbackKey();
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
        string key = GetTrendingGlobalFallbackKey();

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

    public async Task<int> MergeDirtyDeltasAsync(
        int scanBatchSize,
        int processBatchSize,
        CancellationToken ct)
    {
        var processedCount = 0;
        long cursor = 0;

        var server = _database.Multiplexer.GetServer(
            _database.Multiplexer.GetEndPoints().First()
        );

        var allDeltaKeys = new List<RedisKey>();

        do
        {
            var keysPage = server.KeysAsync(
                database: _database.Database,
                pattern: "rank:delta:*",
                pageSize: scanBatchSize,
                cursor: cursor
            );

            // Vì hàm trên fetch keys theo từng trang => nên phải await từng trang, ko phải lấy tất cả keys trong server ra cùng lúc 
            await foreach (var key in keysPage.WithCancellation(ct))
            {
                allDeltaKeys.Add(key);
            }

            cursor = ((IScanningCursor)keysPage).Cursor;
        } while (cursor != 0 && !ct.IsCancellationRequested);

        if (ct.IsCancellationRequested) return processedCount;

        foreach (var chunk in allDeltaKeys.Chunk(processBatchSize))
        {
            var tasks = chunk.Select(deltaKey => ProcessSingleDeltaAsync(deltaKey, ct));
            await Task.WhenAll(tasks);
            processedCount += chunk.Length;
        }

        return processedCount;
    }

    /// <summary>
    /// Hàm helper để xử lý gộp delta cho 1 post
    /// (Đọc từ Primary, không dùng PreferReplica)
    /// </summary>
    private async Task ProcessSingleDeltaAsync(RedisKey deltaKey, CancellationToken ct)
    {
        // 1. Lấy PostId từ key "rank:delta:{postId}"
        if (!Guid.TryParse(deltaKey.ToString().Split(':').LastOrDefault(), out var postId))
        {
            await _database.KeyDeleteAsync(deltaKey); // Xóa key rác
            return;
        }

        var rankKey = GetRankKey(postId); // "rank:{postId}"

        // 2. ĐỌC rank:delta:{postId} (HGETALL)
        var deltaEntries = await _database.HashGetAllAsync(deltaKey);
        var deltaMap = deltaEntries.ToDictionary(
            x => x.Name.ToString(),
            x => x.Value 
        );

        // 3. ĐỌC rank:{postId} (GET) - Đọc từ Primary (mặc định)
        var rankVal = await _database.StringGetAsync(rankKey);
        if (rankVal.IsNullOrEmpty)
        {
            // Rank chính không còn (đã hết hạn?) -> Xóa delta
            await _database.KeyDeleteAsync(deltaKey);
            return;
        }

        var pack = MessagePackSerializer.Deserialize<PostRankPack>(rankVal, MtOpts);

        // 4. Gộp dữ liệu thô (Raw data)
        pack.Reactions += (int)deltaMap.GetValueOrDefault("reactions", 0);
        pack.Comments += (int)deltaMap.GetValueOrDefault("comments", 0);
        pack.Clicks += (int)deltaMap.GetValueOrDefault("clicks", 0);
        pack.Impressions += (int)deltaMap.GetValueOrDefault("impressions", 0);

        // Dùng key "view_duration" như trong code Increment...
        pack.ViewDurationSec += (long)deltaMap.GetValueOrDefault("view_duration", 0);

        pack.UpdatedAtSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // TUYỆT ĐỐI KHÔNG TÍNH `Score` Ở ĐÂY. Để cho RankPostsAsync làm.

        // 5. Ghi đè và Xóa delta (dùng Transaction)
        var newRankBytes = MessagePackSerializer.Serialize(pack, MtOpts, ct);
        var tran = _database.CreateTransaction();

        // Ghi đè rank chính
        tran.StringSetAsync(rankKey, newRankBytes, TimeSpan.FromDays(7));
        // Xóa key delta
        tran.KeyDeleteAsync(deltaKey);

        await tran.ExecuteAsync();
    }

    private static string GetRankKey(Guid postId) => $"rank:{postId}";
    private static string GetTrendingKey(DateTimeOffset date) => $"trending:{date:yyyyMMdd}";
    private static string GetTrendingGlobalFallbackKey() => "trending:global_fallback";
}