using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using UserMemory.API.Protos;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Shared.Services;

public class UserMemorySearchService(
    IEmbeddingService embedder,
    ILogger<UserMemorySearchService> logger
    ) : Protos.UserMemorySearchService.UserMemorySearchServiceBase
{
    public override async Task<SearchMemoriesResponse> SearchMemories(SearchMemoriesRequest req, ServerCallContext ctx)
    {
        // ===== 1) Validate tối thiểu =====
        if (string.IsNullOrWhiteSpace(req.AliasId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "alias_id is required"));

        if (string.IsNullOrWhiteSpace(req.Query))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "query is required"));

        var aliasId = Guid.Parse(req.AliasId);
        var q = req.Query.Trim();

        var topK = req.TopK > 0 ? req.TopK : 8;
        var minScore = req.MinScore > 0 ? req.MinScore : 0.5;

        // ===== 2) Gọi embedder (đã làm vector search + Include Tags) =====
        // Overfetch nhẹ để còn lọc theo score/tag
        var overfetch = Math.Max(5, topK / 2);
        var rawHits = await embedder.SearchAsync(aliasId, q, topK + overfetch, ctx.CancellationToken);

        // ===== 3) Lọc theo tag_codes (OR) nếu có =====
        IEnumerable<(Models.UserMemory row, double score)> filtered = rawHits;

        if (req.TagCodes.Count > 0)
        {
            var set = req.TagCodes
                         .Where(s => !string.IsNullOrWhiteSpace(s))
                         .Select(s => s.Trim())
                         .ToHashSet(StringComparer.OrdinalIgnoreCase);

            filtered = filtered.Where(h =>
                h.row.MemoryTags.Any(t => set.Contains(t.Code)));
        }

        // ===== 4) Lọc theo min_score, sắp xếp & cắt topK =====
        var finalHits = filtered
            .Where(h => h.score >= minScore)
            .OrderByDescending(h => h.score)
            .Take(topK)
            .ToList();

        // ===== 5) Map về response =====
        var resp = new SearchMemoriesResponse();
        foreach (var (row, score) in finalHits)
        {
            resp.Hits.Add(new MemoryHit
            {
                Id = row.Id.ToString(),
                Summary = row.Summary,
                Score = score,
                CapturedAt = row.CreatedAt.ToTimestamp(),
            });
            
            logger.LogInformation("SearchMemories: AliasId={AliasId} Query='{Query}' HitId={HitId} Score={Score}",
                aliasId, q, row.Id, score);
        }

        return resp;
    }
}
