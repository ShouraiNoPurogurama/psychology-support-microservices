using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Services;

public interface ICounterSynchronizationService
{
    Task SynchronizePostCountersAsync(Guid postId, CancellationToken cancellationToken = default);
    Task SynchronizeAllCountersAsync(CancellationToken cancellationToken = default);
    Task RefreshCacheFromDatabaseAsync(Guid postId, CancellationToken cancellationToken = default);
}

public class CounterSynchronizationService : ICounterSynchronizationService
{
    private readonly IPostDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CounterSynchronizationService> _logger;

    public CounterSynchronizationService(
        IPostDbContext context,
        IDistributedCache cache,
        ILogger<CounterSynchronizationService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task SynchronizePostCountersAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        try
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

            if (post == null)
            {
                _logger.LogWarning("Post {PostId} not found for counter synchronization", postId);
                return;
            }

            // Calculate actual counts from database
            var reactionCount = await _context.Reactions
                .Where(r => r.Target.TargetType == ReactionTargetType.Post && 
                           r.Target.TargetId == postId && 
                           !r.IsDeleted)
                .CountAsync(cancellationToken);

            var commentCount = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .CountAsync(cancellationToken);

            // Update database counters
            post.SynchronizeCounters(reactionCount, commentCount);

            // Update Redis cache
            await UpdateCacheCounters(postId, reactionCount, commentCount, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Synchronized counters for post {PostId}: Reactions={ReactionCount}, Comments={CommentCount}",
                postId, reactionCount, commentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing counters for post {PostId}", postId);
            throw;
        }
    }

    public async Task SynchronizeAllCountersAsync(CancellationToken cancellationToken = default)
    {
        var batchSize = 100;
        var skip = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var postIds = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Skip(skip)
                .Take(batchSize)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (!postIds.Any())
                break;

            foreach (var postId in postIds)
            {
                await SynchronizePostCountersAsync(postId, cancellationToken);
            }

            skip += batchSize;
            _logger.LogInformation("Processed {Count} posts for counter synchronization", postIds.Count);

            // Add small delay to avoid overwhelming the system
            await Task.Delay(100, cancellationToken);
        }
    }

    public async Task RefreshCacheFromDatabaseAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post != null)
        {
            await UpdateCacheCounters(
                postId, 
                post.Metrics.ReactionCount, 
                post.Metrics.CommentCount, 
                cancellationToken);
        }
    }

    private async Task UpdateCacheCounters(
        Guid postId, 
        int reactionCount, 
        int commentCount, 
        CancellationToken cancellationToken)
    {
        var counters = new
        {
            ReactionCount = reactionCount,
            CommentCount = commentCount,
            LastUpdated = DateTime.UtcNow
        };

        var cacheKey = $"post:{postId}:counters";
        var serializedCounters = JsonSerializer.Serialize(counters);
        
        await _cache.SetStringAsync(
            cacheKey, 
            serializedCounters, 
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            }, 
            cancellationToken);
    }
}
