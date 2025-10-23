using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Dtos;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Feed.Infrastructure.Data.Processors;

/// <summary>
/// Background job to populate the global fallback Redis Sorted Set.
/// Should run hourly to maintain a stable list of popular posts.
/// Uses Quartz.NET or Hangfire for scheduling.
/// </summary>
[DisallowConcurrentExecution]
public sealed class UpdateGlobalFallbackJob : IJob // 
{
    private readonly IRankingService _rankingService;
    private readonly IPostReadRepository _postReadRepository;
    private readonly ILogger<UpdateGlobalFallbackJob> _logger;

    public UpdateGlobalFallbackJob(
        IRankingService rankingService,
        IPostReadRepository postReadRepository,
        ILogger<UpdateGlobalFallbackJob> logger)
    {
        _rankingService = rankingService;
        _postReadRepository = postReadRepository;
        _logger = logger;
    }

    /// <summary>
    /// Execute the job to update the global fallback list.
    /// Logic:
    /// 1. Scan posts from the past 30 days in batches of 7 days
    /// 2. Calculate engagement scores (reactions + comments * 2 + ctr * 100)
    /// 3. Take top 500 posts
    /// 4. Update trending:global_fallback Redis Sorted Set
    /// </summary>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting UpdateGlobalFallbackJob.");

        try
        {
            var topPosts = await GetTopPostsByScanningBackwardsAsync(context.CancellationToken);
            _logger.LogInformation("Fetched {Count} top posts to update global fallback.", topPosts.Count);


            if (topPosts.Count > 0)
            {
                await _rankingService.UpdateGlobalFallbackAsync(topPosts);
                _logger.LogInformation("Successfully updated global fallback with {Count} posts.", topPosts.Count);
            }
            else
            {
                _logger.LogWarning("No posts found for global fallback update. The fallback list might be empty.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while updating global fallback.");
            throw; // Rethrow to let Quartz handle the job failure
        }
    }

    /// <summary>
    /// Scan backwards theo cửa sổ ngày, bulk-read rank, tính điểm + decay,
    /// giữ Top-K bằng min-heap để tránh sort toàn bộ.
    /// </summary>
    private async Task<IReadOnlyList<(Guid PostId, double Score)>> GetTopPostsByScanningBackwardsAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning backwards to build global fallback (Top {K})",
            UpdateGlobalFallbackJobConfiguration.TopPostsLimit);

        var k = UpdateGlobalFallbackJobConfiguration.TopPostsLimit;
        var maxLookbackDays = UpdateGlobalFallbackJobConfiguration.MaxLookbackDays;
        var windowDays = UpdateGlobalFallbackJobConfiguration.ScanWindowDays;

        //Min-heap: luôn giữ phần tử có priority (score) nhỏ nhất ở đỉnh
        var heap = new PriorityQueue<(Guid PostId, double Score), double>();
        var seen = new HashSet<Guid>();
        var currentDayOffset = 0;
        var processedCount = 0;
        var consecutiveEmptyWindows = 0;

        const double lambdaPerHour = 0.015; // ~0.34 sau 72h
        const double floorDecay = 0.25; // sàn để không “mất hút”

        while (heap.Count < k && currentDayOffset < maxLookbackDays)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Job cancellation requested. Stopping scan.");
                break;
            }

            _logger.LogDebug("Fetching posts: window={WindowDays}d, startOffset={Offset}d",
                windowDays, currentDayOffset);

            IReadOnlyList<PostInfo> postInfos = await _postReadRepository.GetMostRecentPublicPostsAsync(
                days: windowDays,
                limit: UpdateGlobalFallbackJobConfiguration.BatchSize,
                startDayOffset: currentDayOffset,
                cancellationToken: cancellationToken
            );

            if (postInfos.Count == 0)
            {
                consecutiveEmptyWindows++;
                // dừng sớm khi cửa sổ rỗng
                if (consecutiveEmptyWindows >= 1)
                {
                    _logger.LogInformation("No posts found in window at offset {Offset}d. Stop scanning.", currentDayOffset);
                    break;
                }

                currentDayOffset += windowDays;
                continue;
            }

            consecutiveEmptyWindows = 0;

            // Bulk-read rank cho cả batch
            var ids = postInfos
                .Select(p => p.PostId)
                .Where(id => seen.Add(id))
                .ToList();

            if (ids.Count == 0)
            {
                currentDayOffset += windowDays;
                continue;
            }

            var rankMap = await _rankingService.GetPostRanksAsync(ids);
            var infoMap = postInfos.ToDictionary(p => p.PostId, p => p); 

            foreach (var postId in ids)
            {
                if (rankMap.TryGetValue(postId, out var rank))
                {
                    var blended = ComputeBlendedScore(
                        reactions: rank.Reactions,
                        comments : rank.Comments,
                        ctr      : rank.Ctr,
                        createdAtUtc: rank.CreatedAt
                    );
                    PushTopK(heap, k, postId, blended);
                    processedCount++;
                }
            }
            
            var missingIds = ids.Where(id => !rankMap.ContainsKey(id)).ToArray();
            foreach (var mid in missingIds)
            {
                if (!infoMap.TryGetValue(mid, out var info)) continue;
                var recencyScore = ComputeRecencyOnlyScore(info.CreatedAt);
                PushTopK(heap, k, mid, recencyScore);
            }

            // Log gọn: kích thước heap + min score hiện tại
            if (heap.TryPeek(out _, out var minScoreNow))
            {
                _logger.LogDebug("Heap={HeapCount}/{K}, minScore~{MinScore:F2}, processed={Processed}",
                    heap.Count, k, minScoreNow, processedCount);
            }
            else
            {
                _logger.LogDebug("Heap=0/{K}, processed={Processed}", k, processedCount);
            }

            currentDayOffset += windowDays;
        }

        if (heap.Count == 0)
        {
            _logger.LogWarning("No posts with rank data could be found.");
            return Array.Empty<(Guid, double)>();
        }

        //Rút Top-K từ min-heap (bé nhất ra trước) -> đảo lại cho giảm dần
        var top = new List<(Guid PostId, double Score)>(heap.Count);
        while (heap.Count > 0)
        {
            var item = heap.Dequeue(); // (PostId, Score)
            top.Add(item);
        }

        top.Reverse();

        //Nếu heap < k vì dữ liệu ít, vẫn trả về những gì có
        _logger.LogInformation("Built global fallback: selected={Sel}, processed={Processed}, score range {Min:F2}..{Max:F2}",
            top.Count, processedCount, top.Last().Score, top.First().Score);

        return top;
    }


    //Giữ Top-K theo score lớn nhất bằng min-heap (PriorityQueue là min-heap theo priority)
    private static void PushTopK(
        PriorityQueue<(Guid PostId, double Score), double> heap,
        int k,
        Guid id,
        double score)
    {
        if (heap.Count < k)
        {
            heap.Enqueue((id, score), score);
            return;
        }

        // Có đủ K rồi: chỉ thay nếu score mới > min hiện tại
        if (heap.TryPeek(out _, out var minScore) && score > minScore)
        {
            heap.Dequeue();
            heap.Enqueue((id, score), score);
        }
    }
    private static double ComputeBlendedScore(
        double reactions,
        double comments,
        double ctr,
        DateTimeOffset createdAtUtc)
    {
        var ageH = (DateTimeOffset.UtcNow - createdAtUtc).TotalHours;

        // 1) Recency-only
        var recencyScore = ComputeRecencyOnlyScore(createdAtUtc); // ~0..100

        // 2) Engagement + decay + prior
        const double engagementPrior = 5.0;          // tune: 2–10
        var engagement = reactions + 2.0 * comments + 100.0 * ctr + engagementPrior;

        const double lambdaPerHour = 0.015;          
        const double floorDecay   = 0.25;
        var decay = Math.Max(floorDecay, Math.Exp(-lambdaPerHour * ageH));
        var engagementDecayScore = engagement * decay;

        // 3) Trọng số chuyển pha theo tuổi bài (logistic)
        const double halfLifeHours = 24.0;           // sau ~24h bắt đầu bớt ưu tiên recency
        const double softnessHours = 6.0;            // độ dốc chuyển pha
        var wRecency = 1.0 / (1.0 + Math.Exp((ageH - halfLifeHours) / softnessHours));
        // ageH = 0h  -> w ~ 0.98 (gần như recency)
        // ageH = 24h -> w ~ 0.5
        // ageH >>    -> w ~ 0 (nghiêng engagement)

        return wRecency * recencyScore + (1.0 - wRecency) * engagementDecayScore;
    }
    
    private static double ComputeRecencyOnlyScore(DateTimeOffset createdAtUtc)
    {
        //decay mượt theo giờ: exp(-λ * ageH), scale để điểm có ý nghĩa
        const double lambdaPerHour = 0.02;     // nhạy vừa phải
        const double scale = 100.0;            //để cạnh tranh với baseScore thường
        var ageH = (DateTimeOffset.UtcNow - createdAtUtc).TotalHours;
        var decay = Math.Exp(-lambdaPerHour * Math.Max(0, ageH));
        return scale * decay;                  //chỉ dựa recency
    }

}