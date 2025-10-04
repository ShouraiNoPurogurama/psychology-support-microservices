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
public sealed class UpdateGlobalFallbackJob : IJob // <-- Implement IJob
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
    /// 1. Scan posts from the last 72 hours
    /// 2. Calculate engagement scores (reactions + comments * 2 + ctr * 100)
    /// 3. Take top 500 posts
    /// 4. Update trending:global_fallback Redis Sorted Set
    /// </summary>

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting UpdateGlobalFallbackJob");

        try
        {
            var topPosts = await GetTopPostsFromLast72HoursAsync();
            
            if (topPosts.Count > 0)
            {
                await _rankingService.UpdateGlobalFallbackAsync(topPosts);
                _logger.LogInformation("Successfully updated global fallback with {Count} posts", topPosts.Count);
            }
            else
            {
                _logger.LogWarning("No posts found for global fallback update. Consider checking data sources.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating global fallback");
            throw;
        }
    }
    
    /// <summary>
    /// Fetch and score posts from the last 72 hours.
    /// This implementation provides a more complete approach.
    /// </summary>
    private async Task<IReadOnlyList<(Guid PostId, double Score)>> GetTopPostsFromLast72HoursAsync()
    {
        _logger.LogDebug("Fetching posts from last 72 hours for global fallback calculation");

        try
        {
            // Get recent public posts from the database
            // In production, this would query posts from the last 72 hours
            // For now, we get the most recent posts available
            var recentPostIds = await _postReadRepository.GetMostRecentPublicPostsAsync(
                UpdateGlobalFallbackJobConfiguration.TopPostsLimit * 2); // Get 2x to ensure we have enough after filtering);

            if (recentPostIds.Count == 0)
            {
                _logger.LogWarning("No recent posts found in database for global fallback");
                return Array.Empty<(Guid, double)>();
            }

            _logger.LogDebug("Found {Count} recent posts, calculating scores", recentPostIds.Count);

            // Fetch rank data for each post and calculate score
            var scoredPosts = new List<(Guid PostId, double Score, DateTimeOffset UpdatedAt)>();
            
            foreach (var postId in recentPostIds)
            {
                var rankData = await _rankingService.GetPostRankAsync(postId);
                
                if (rankData is not null)
                {
                    // Calculate engagement score:
                    // - Each reaction: 1 point
                    // - Each comment: 2 points (more valuable than reactions)
                    // - CTR (click-through rate): 100 points per 1.0 CTR
                    var score = rankData.Reactions + (rankData.Comments * 2.0) + (rankData.Ctr * 100.0);
                    
                    // Apply time decay: newer posts get a slight boost
                    var ageInHours = (DateTimeOffset.UtcNow - rankData.UpdatedAt).TotalHours;
                    var timeDecayFactor = Math.Max(0.5, 1.0 - (ageInHours / (72.0 * 2))); // Decay over 144 hours (6 days)
                    var adjustedScore = score * timeDecayFactor;
                    
                    scoredPosts.Add((postId, adjustedScore, rankData.UpdatedAt));
                }
            }

            if (scoredPosts.Count == 0)
            {
                _logger.LogWarning("No scored posts available - rank data may be missing");
                return Array.Empty<(Guid, double)>();
            }

            // Sort by score descending and take top N
            var topPosts = scoredPosts
                .OrderByDescending(p => p.Score)
                .Take(UpdateGlobalFallbackJobConfiguration.TopPostsLimit)
                .Select(p => (p.PostId, p.Score))
                .ToList();

            _logger.LogInformation(
                "Calculated scores for {TotalCount} posts, selected top {TopCount} for global fallback. " +
                "Score range: {MinScore:F2} - {MaxScore:F2}",
                scoredPosts.Count,
                topPosts.Count,
                topPosts.LastOrDefault().Score,
                topPosts.FirstOrDefault().Score);

            return topPosts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating top posts from last 72 hours");
            throw;
        }
    }
    
}

/// <summary>
/// Configuration for the UpdateGlobalFallbackJob.
/// Register this with Quartz.NET or Hangfire to run hourly.
/// </summary>
/// <example>
/// Quartz.NET configuration:
/// <code>
/// services.AddQuartz(q =>
/// {
///     var jobKey = new JobKey(UpdateGlobalFallbackJobConfiguration.JobName);
///     q.AddJob&lt;UpdateGlobalFallbackJob&gt;(opts => opts.WithIdentity(jobKey));
///     q.AddTrigger(opts => opts
///         .ForJob(jobKey)
///         .WithIdentity($"{UpdateGlobalFallbackJobConfiguration.JobName}-trigger")
///         .WithCronSchedule(UpdateGlobalFallbackJobConfiguration.CronExpression));
/// });
/// </code>
/// 
/// Hangfire configuration:
/// <code>
/// RecurringJob.AddOrUpdate&lt;UpdateGlobalFallbackJob&gt;(
///     UpdateGlobalFallbackJobConfiguration.JobName,
///     job => job.ExecuteAsync(CancellationToken.None),
///     Cron.Hourly);
/// </code>
/// </example>
public static class UpdateGlobalFallbackJobConfiguration
{
    public const string JobName = "UpdateGlobalFallbackJob";
    public const string CronExpression = "0 0 * * * ?"; // Every hour at minute 0
    public static readonly TimeSpan LookbackPeriod = TimeSpan.FromHours(72);
    public const int TopPostsLimit = 500;
    
    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    public static void Validate()
    {
        if (TopPostsLimit <= 0)
            throw new InvalidOperationException($"{nameof(TopPostsLimit)} must be greater than 0");
        
        if (LookbackPeriod.TotalHours <= 0)
            throw new InvalidOperationException($"{nameof(LookbackPeriod)} must be greater than 0");
    }
}
