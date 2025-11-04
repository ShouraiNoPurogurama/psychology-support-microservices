using BuildingBlocks.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Pgvector;
using UserMemory.API.Data;
using UserMemory.API.Shared.Dtos.Gemini.Embedding;
using UserMemory.API.Shared.Enums;
using UserMemory.API.Shared.Services.Contracts;
using UserMemory.API.Shared.Utils;

namespace UserMemory.API.Shared.Services;

using UserMemory = UserMemory.API.Models.UserMemory;

public class GeminiEmbeddingService(
    ILogger<GeminiEmbeddingService> logger,
    IGeminiClient geminiClient,
    UserMemoryDbContext db,
    ITagResolverService tagResolverService,
    IOptions<GeminiEmbeddingOptions> opt) : IEmbeddingService
{
    private readonly GeminiEmbeddingOptions _opt = opt.Value;

    // ===== 1) AddMemory với ENUMS (khuyến nghị) =====
    public async Task<UserMemory> UpsertMemoryAsync(
        Guid aliasId,
        string summary,
        string[] tagCodes,
        CancellationToken ct = default)
    {
        // 0) Chuẩn hoá text để bắt trùng tuyệt đối (optional)
        var normText = NormalizeTextForHash(summary);
        var textHash = Sha256(normText);

        // 1) Embed
        var vec = await geminiClient.EmbedTextAsync(summary, _opt.OutputDimensionality, "RETRIEVAL_DOCUMENT", ct);
        var final = _opt.Normalize ? Normalize(vec) : vec;

        // 2) Nếu bật dedup → kiểm tra trùng
        UserMemory? dup = null;
        if (_opt.DedupEnabled)
        {
            // 2.a) Exact-text (nhanh, optional nếu chưa có cột hash)
            dup = await db.UserMemories
                .Include(x => x.MemoryTags)
                .Where(x => x.AliasId == aliasId && x.Summary.ToLower() == normText)
                .FirstOrDefaultAsync(ct);

            // 2.b) Vector near-duplicate nếu chưa match exact
            if (dup is null)
            {
                var (row, score) = await TryFindVectorDuplicateAsync(aliasId, final, _opt.VectorSimThreshold, ct);
                dup = row;
            }
        }

        // 3) Resolve tags
        var tagMap = await tagResolverService.ResolveByCodesAsync(tagCodes, ct);
        if (dup is not null)
        {
            // 4) Merge tag nếu trùng
            if (_opt.MergeTagsOnDuplicate)
            {
                var have = dup.MemoryTags.ToDictionary(t => t.Id, t => t);
                foreach (var tag in tagMap.Values)
                    if (!have.ContainsKey(tag.Id))
                        dup.MemoryTags.Add(tag);
            }

            if (_opt.TouchTimestampOnDuplicate)
            {
                dup.LastModified = DateTimeOffset.UtcNow;
                dup.LastModifiedBy = SystemActors.SystemUUID.ToString();
            }

            await db.SaveChangesAsync(ct);
            return dup;
        }

        // 5) Insert mới nếu không trùng
        var um = new UserMemory
        {
            Id = Guid.NewGuid(),
            AliasId = aliasId,
            Summary = summary,
            Embedding = new Vector(final),
            CreatedAt = DateTimeOffset.UtcNow
            // SummaryHash = textHash   // nếu bạn thêm cột
        };

        foreach (var tag in tagMap.Values)
            um.MemoryTags.Add(tag);

        db.UserMemories.Add(um);
        await db.SaveChangesAsync(ct);
        return um;
    }

    // ===== 2) Batch insert với ENUMS =====
    public async Task<IReadOnlyList<UserMemory>> AddMemoriesBatchAsync(
        Guid aliasId,
        IEnumerable<(string summary, TopicTag[] topics, EmotionTag[] emotions, RelationshipTag[] relations)> items,
        CancellationToken ct = default)
    {
        var listItems = items.ToList();

        // 1) Embed batch
        var texts = listItems.Select(i => i.summary).ToArray();
        var vectors = await geminiClient.EmbedBatchAsync(texts, _opt.OutputDimensionality, _opt.TaskType, ct);

        // 2) Build all codes → resolve 1 lần
        var allCodes = listItems
            .SelectMany(i => i.topics.Select(x => x.ToCode())
                .Concat(i.emotions.Select(x => x.ToCode()))
                .Concat(i.relations.Select(x => x.ToCode())))
            .Distinct()
            .ToArray();

        var tagMap = await tagResolverService.ResolveByCodesAsync(allCodes, ct);

        // 3) Map & create
        var result = new List<UserMemory>(listItems.Count);
        for (int i = 0; i < listItems.Count; i++)
        {
            var item = listItems[i];
            var final = _opt.Normalize ? Normalize(vectors[i]) : vectors[i];

            var um = new UserMemory
            {
                Id = Guid.NewGuid(),
                AliasId = aliasId,
                Summary = item.summary,
                Embedding = new Vector(final),
                CreatedAt = DateTimeOffset.UtcNow
            };

            foreach (var code in item.topics.Select(x => x.ToCode())
                         .Concat(item.emotions.Select(x => x.ToCode()))
                         .Concat(item.relations.Select(x => x.ToCode())))
            {
                if (tagMap.TryGetValue(code, out var tag))
                    um.MemoryTags.Add(tag);
            }

            result.Add(um);
        }

        db.UserMemories.AddRange(result);
        await db.SaveChangesAsync(ct);
        return result;
    }

    // ===== 2b) Batch overload nếu vẫn truyền code string =====
    public async Task<IReadOnlyList<UserMemory>> AddMemoriesBatchAsync(
        Guid aliasId, IEnumerable<(string summary, string[] tagCodes)> items, CancellationToken ct = default)
    {
        var listItems = items.ToList();

        var texts = listItems.Select(i => i.summary).ToArray();
        var vectors = await geminiClient.EmbedBatchAsync(texts, _opt.OutputDimensionality, _opt.TaskType, ct);

        var allCodes = listItems.SelectMany(i => i.tagCodes).Distinct().ToArray();
        var tagMap = await tagResolverService.ResolveByCodesAsync(allCodes, ct);

        var result = new List<UserMemory>(listItems.Count);
        for (int i = 0; i < listItems.Count; i++)
        {
            var item = listItems[i];
            var final = _opt.Normalize ? Normalize(vectors[i]) : vectors[i];

            var um = new UserMemory
            {
                Id = Guid.NewGuid(),
                AliasId = aliasId,
                Summary = item.summary,
                Embedding = new Vector(final),
                CreatedAt = DateTimeOffset.UtcNow
            };

            foreach (var code in item.tagCodes)
                if (tagMap.TryGetValue(code, out var tag))
                    um.MemoryTags.Add(tag);

            result.Add(um);
        }

        db.UserMemories.AddRange(result);
        await db.SaveChangesAsync(ct);
        return result;
    }

    /// <summary>
    /// Vector search trong phạm vi 1 alias. Trả entity kèm score.
    /// Lưu ý: vì không còn cột tags[], ta tách 2 bước:
    /// (a) RAW SQL lấy (id, score) theo ivfflat
    /// (b) Query lại theo id để Include Tags (many-to-many)
    /// </summary>
    public async Task<IReadOnlyList<(UserMemory row, double score)>> SearchAsync(
        Guid aliasId, string query, int topK = 10, CancellationToken ct = default)
    {
        // 1) Embed query
        var q = await geminiClient.EmbedTextAsync(query, _opt.OutputDimensionality, _opt.TaskType, ct);
        var qFinal = _opt.Normalize ? Normalize(q) : q;

        // 2) Bước (a): chỉ lấy id & score để tận dụng index <-> (không Include ở đây)
        var sql = """
                  SELECT 
                      um.id, 
                      (1 - (um.embedding <=> @q)) AS similarity_score,
                      COALESCE(um.last_modified, um.created_at) as effective_date
                  FROM user_memories um
                  WHERE um.alias_id = @alias
                  ORDER BY 
                      um.embedding <=> @q,  -- 1. Ưu tiên KHOẢNG CÁCH (Cosine)
                      effective_date DESC   -- 2. Ưu tiên NGÀY MỚI NHẤT
                  LIMIT @k
                  """;

        var pQ = new NpgsqlParameter<Vector>("q", new Vector(qFinal));
        var pAlias = new NpgsqlParameter("alias", aliasId);
        var pK = new NpgsqlParameter("k", topK);

        var hits = await db.Database.SqlQueryRaw<HitRow>(sql, pQ, pAlias, pK).ToListAsync(ct);
        if (hits.Count == 0) return Array.Empty<(UserMemory, double)>();

        var ids = hits.Select(h => h.id).ToArray();

        // 3) Bước (b): load entities + Tags
        var entities = await db.UserMemories
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.MemoryTags)
            .ToListAsync(ct);

        // 4) Stitch theo thứ tự score
        var map = entities.ToDictionary(x => x.Id, x => x);
        var result = new List<(UserMemory, double)>(hits.Count);
        foreach (var h in hits)
            if (map.TryGetValue(h.id, out var ent))
                result.Add((ent, h.similarity_score));

        return result;
    }

    private async Task<(UserMemory? row, double score)> TryFindVectorDuplicateAsync(
        Guid aliasId, float[] qFinal, double threshold, CancellationToken ct)
    {
        var sql = """
                  SELECT um.id, (1 - (um.embedding <=> @q)) AS similarity_score
                  FROM user_memories um
                  WHERE um.alias_id = @alias
                  ORDER BY um.embedding <=> @q 
                  LIMIT 1
                  """;

        var pQ = new NpgsqlParameter<Vector>("q", new Vector(qFinal));
        var pAlias = new NpgsqlParameter("alias", aliasId);

        var hit = await db.Database.SqlQueryRaw<HitRow>(sql, pQ, pAlias).FirstOrDefaultAsync(ct);

        logger.LogInformation("Duplicate check: found id={Id} with score={Score}", hit?.id, hit?.similarity_score);

        if (hit is null || hit.similarity_score < threshold) return (null, 0);

        var ent = await db.UserMemories
            .Where(x => x.Id == hit.id)
            .Include(x => x.MemoryTags)
            .FirstOrDefaultAsync(ct);

        return (ent, hit.similarity_score);
    }


    private static float[] Normalize(float[] v)
    {
        if (v.Length == 0) return v;
        var norm = MathF.Sqrt(v.Sum(x => x * x));
        if (norm <= 0f) return v;
        var r = new float[v.Length];
        for (int i = 0; i < v.Length; i++) r[i] = v[i] / norm;
        return r;
    }

    private static string NormalizeTextForHash(string s)
    {
        var t = s.Trim().ToLowerInvariant();
        t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ");
        return t;
    }

    private static string Sha256(string text)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash); // AABBCC...
    }
}