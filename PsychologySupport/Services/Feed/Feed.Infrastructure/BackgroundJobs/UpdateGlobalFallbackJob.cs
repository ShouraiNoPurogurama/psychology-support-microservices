using Feed.Application.Abstractions.RankingService;
using Feed.Application.Abstractions.PostRepository;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Feed.Infrastructure.BackgroundJobs;

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
    /// Fetch and score 500 most recent posts 
    /// This implementation provides a more complete approach.
    /// </summary>
     private async Task<IReadOnlyList<(Guid PostId, double Score)>> GetTopPostsByScanningBackwardsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning backwards in time to find top posts for global fallback.");

        var scoredPosts = new List<(Guid PostId, double Score)>();
        var processedPostIds = new HashSet<Guid>();

        var currentDayOffset = 0;
        var targetCount = UpdateGlobalFallbackJobConfiguration.TopPostsLimit;

        // Vòng lặp sẽ chạy cho đến khi đủ số lượng bài viết hoặc quét quá giới hạn thời gian
        while (scoredPosts.Count < targetCount && currentDayOffset < UpdateGlobalFallbackJobConfiguration.MaxLookbackDays)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Job cancellation requested. Stopping scan.");
                break;
            }

            _logger.LogDebug(
                "Fetching post batch with window size of {WindowDays} days, starting {Offset} days ago.",
                UpdateGlobalFallbackJobConfiguration.ScanWindowDays,
                currentDayOffset);

            var postInfos = await _postReadRepository.GetMostRecentPublicPostsAsync(
                days: UpdateGlobalFallbackJobConfiguration.ScanWindowDays,
                limit: UpdateGlobalFallbackJobConfiguration.BatchSize,
                startDayOffset: currentDayOffset,
                cancellationToken: cancellationToken
            );

            if (postInfos.Count == 0 && currentDayOffset > 0)
            {
                _logger.LogInformation("No more posts found in the database. Stopping scan.");
                break; 
            }

            // Tính điểm cho các bài viết vừa tìm được
            foreach (var post in postInfos)
            {
                if (processedPostIds.Contains(post.PostId)) continue;
                
                var rankData = await _rankingService.GetPostRankAsync(post.PostId);
                if (rankData is not null)
                {
                    var score = rankData.Reactions + (rankData.Comments * 2.0) + (rankData.Ctr * 100.0);
                    var ageInHours = (DateTimeOffset.UtcNow - rankData.UpdatedAt).TotalHours;
                    var timeDecayFactor = Math.Max(0.5, 1.0 - (ageInHours / (72.0 * 2)));
                    var adjustedScore = score * timeDecayFactor;

                    scoredPosts.Add((post.PostId, adjustedScore));
                }
                processedPostIds.Add(post.PostId);
            }

            _logger.LogDebug("Current collected scored posts: {Count}", scoredPosts.Count);
            
            // Tăng offset để quét khoảng thời gian tiếp theo trong quá khứ
            currentDayOffset += UpdateGlobalFallbackJobConfiguration.ScanWindowDays;
        }

        if (scoredPosts.Count == 0)
        {
             _logger.LogWarning("No posts with rank data could be found.");
             return Array.Empty<(Guid, double)>();
        }
            
        if (scoredPosts.Count < targetCount)
        {
            _logger.LogWarning("Could not find {TargetCount} posts. Found only {FoundCount} posts after scanning back {MaxDays} days.", 
                targetCount, scoredPosts.Count, UpdateGlobalFallbackJobConfiguration.MaxLookbackDays);
        }

        var topPosts = scoredPosts
            .OrderByDescending(p => p.Score)
            .Take(targetCount)
            .ToList();
        
        _logger.LogInformation(
            "Calculated scores for {TotalCount} posts, selected top {TopCount}. Score range: {MinScore:F2} - {MaxScore:F2}",
            scoredPosts.Count, topPosts.Count, topPosts.LastOrDefault().Score, topPosts.FirstOrDefault().Score);

        return topPosts;
    }
    
}