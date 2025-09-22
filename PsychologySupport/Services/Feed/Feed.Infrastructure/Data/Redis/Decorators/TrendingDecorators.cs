using System.Diagnostics;
using Feed.Application.Abstractions.Redis;
using Feed.Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using Polly;
using Polly.Retry;

namespace Feed.Infrastructure.Data.Redis.Decorators;

// Key Prefix Decorator
internal sealed class TrendingKeyPrefixDecorator : ITrendingProvider
{
    private readonly ITrendingProvider _inner;
    private readonly string _keyPrefix;

    public TrendingKeyPrefixDecorator(ITrendingProvider inner, IOptions<RedisConfiguration> config)
    {
        _inner = inner;
        _keyPrefix = config.Value.KeyPrefix ?? "feed";
    }

    public Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
        => _inner.AddPostAsync(postId, score, date, ct);

    public Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct)
        => _inner.GetTopPostsAsync(date, count, ct);

    public Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
        => _inner.UpdatePostScoreAsync(postId, score, date, ct);
}

// Retry Decorator with Polly
internal sealed class TrendingRetryDecorator : ITrendingProvider
{
    private readonly ITrendingProvider _inner;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<TrendingRetryDecorator> _logger;

    public TrendingRetryDecorator(ITrendingProvider inner, ILogger<TrendingRetryDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(100)
            })
            .Build();
    }

    public async Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        await _pipeline.ExecuteAsync(async _ => 
        {
            await _inner.AddPostAsync(postId, score, date, ct);
        }, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct)
    {
        return await _pipeline.ExecuteAsync(async _ => 
            await _inner.GetTopPostsAsync(date, count, ct), ct);
    }

    public async Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        await _pipeline.ExecuteAsync(async _ => 
        {
            await _inner.UpdatePostScoreAsync(postId, score, date, ct);
        }, ct);
    }
}

// Metrics Decorator
internal sealed class TrendingMetricsDecorator : ITrendingProvider
{
    private readonly ITrendingProvider _inner;
    private static readonly Meter _meter = new("Feed.Redis.Trending");
    private static readonly Counter<long> _operations = _meter.CreateCounter<long>("trending_operations_total");
    private static readonly Histogram<double> _duration = _meter.CreateHistogram<double>("trending_operation_duration_ms");

    public TrendingMetricsDecorator(ITrendingProvider inner)
    {
        _inner = inner;
    }

    public async Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.AddPostAsync(postId, score, date, ct);
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "add"),
                new KeyValuePair<string, object?>("result", "success"));
        }
        catch
        {
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "add"),
                new KeyValuePair<string, object?>("result", "error"));
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    public async Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _inner.GetTopPostsAsync(date, count, ct);
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "get_top"),
                new KeyValuePair<string, object?>("result", "success"));
            return result;
        }
        catch
        {
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "get_top"),
                new KeyValuePair<string, object?>("result", "error"));
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    public async Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.UpdatePostScoreAsync(postId, score, date, ct);
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "update"),
                new KeyValuePair<string, object?>("result", "success"));
        }
        catch
        {
            _operations.Add(1, new KeyValuePair<string, object?>("operation", "update"),
                new KeyValuePair<string, object?>("result", "error"));
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }
}

// Logging Decorator
internal sealed class TrendingLoggingDecorator : ITrendingProvider
{
    private readonly ITrendingProvider _inner;
    private readonly ILogger<TrendingLoggingDecorator> _logger;

    public TrendingLoggingDecorator(ITrendingProvider inner, ILogger<TrendingLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task AddPostAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        _logger.LogDebug("Adding post {PostId} with score {Score} to trending {Date}", 
            postId, score, date);
        try
        {
            await _inner.AddPostAsync(postId, score, date, ct);
            _logger.LogDebug("Successfully added post {PostId} to trending", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add post {PostId} to trending", postId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Guid>> GetTopPostsAsync(DateOnly date, int count, CancellationToken ct)
    {
        _logger.LogDebug("Getting top {Count} posts for date {Date}", count, date);
        try
        {
            var result = await _inner.GetTopPostsAsync(date, count, ct);
            _logger.LogDebug("Retrieved {Count} trending posts for date {Date}", result.Count, date);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trending posts for date {Date}", date);
            throw;
        }
    }

    public async Task UpdatePostScoreAsync(Guid postId, double score, DateOnly date, CancellationToken ct)
    {
        _logger.LogDebug("Updating post {PostId} score to {Score} for date {Date}", 
            postId, score, date);
        try
        {
            await _inner.UpdatePostScoreAsync(postId, score, date, ct);
            _logger.LogDebug("Successfully updated post {PostId} score", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update post {PostId} score", postId);
            throw;
        }
    }
}
