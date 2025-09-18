using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Post.Application.Services;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, RateLimitRule rule, CancellationToken cancellationToken = default);
    Task IncrementAsync(string key, RateLimitRule rule, CancellationToken cancellationToken = default);
}

public record RateLimitRule(
    int MaxAttempts,
    TimeSpan TimeWindow,
    string? ErrorMessage = null
);

public class MemoryRateLimitingService : IRateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;

    public MemoryRateLimitingService(IMemoryCache cache, IOptions<RateLimitOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public Task<bool> IsAllowedAsync(string key, RateLimitRule rule, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rate_limit:{key}";
        var attempts = _cache.Get<RateLimitEntry>(cacheKey);

        if (attempts == null)
        {
            return Task.FromResult(true);
        }

        // Clean expired attempts
        attempts.Timestamps.RemoveAll(t => DateTime.UtcNow - t > rule.TimeWindow);

        return Task.FromResult(attempts.Timestamps.Count < rule.MaxAttempts);
    }

    public Task IncrementAsync(string key, RateLimitRule rule, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"rate_limit:{key}";
        var attempts = _cache.Get<RateLimitEntry>(cacheKey) ?? new RateLimitEntry();

        // Clean expired attempts
        attempts.Timestamps.RemoveAll(t => DateTime.UtcNow - t > rule.TimeWindow);

        // Add current attempt
        attempts.Timestamps.Add(DateTime.UtcNow);

        // Cache with sliding expiration
        _cache.Set(cacheKey, attempts, rule.TimeWindow);

        return Task.CompletedTask;
    }

    private class RateLimitEntry
    {
        public List<DateTime> Timestamps { get; } = new();
    }
}

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public RateLimitRule PostCreation { get; set; } = new(5, TimeSpan.FromMinutes(1), "Too many posts created. Please wait a moment.");
    public RateLimitRule CommentCreation { get; set; } = new(10, TimeSpan.FromMinutes(1), "Too many comments created. Please slow down.");
    public RateLimitRule ReactionCreation { get; set; } = new(30, TimeSpan.FromMinutes(1), "Too many reactions. Please wait.");
}
