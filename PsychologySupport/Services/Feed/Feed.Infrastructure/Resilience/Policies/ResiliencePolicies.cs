using Feed.Application.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using StackExchange.Redis;

namespace Feed.Infrastructure.Resilience.Policies;

/// <summary>
/// Centralized resilience policies for external dependencies.
/// Implements circuit breaker and retry patterns.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Creates a resilience pipeline for Redis operations.
    /// Includes retry with exponential backoff and circuit breaker.
    /// </summary>
    public static ResiliencePipeline<T> CreateRedisPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(100),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Redis operation failed, attempt {Attempt} of {MaxAttempts}. Retrying after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        3,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                },
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<RedisConnectionException>()
                    .Handle<RedisTimeoutException>()
                    .Handle<TimeoutException>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = FeedConstants.CIRCUIT_BREAKER_FAILURE_THRESHOLD,
                MinimumThroughput = FeedConstants.CIRCUIT_BREAKER_MIN_THROUGHPUT,
                BreakDuration = TimeSpan.FromSeconds(FeedConstants.CIRCUIT_BREAKER_BREAK_DURATION_SECONDS),
                OnOpened = args =>
                {
                    logger.LogError(
                        "Redis circuit breaker opened after {Failures} failures. Breaking for {BreakDuration}s",
                        args.Outcome.Exception?.Message,
                        FeedConstants.CIRCUIT_BREAKER_BREAK_DURATION_SECONDS);
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Redis circuit breaker closed. Normal operations resumed");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Redis circuit breaker half-open. Testing connection");
                    return default;
                },
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<RedisConnectionException>()
                    .Handle<RedisTimeoutException>()
                    .Handle<TimeoutException>()
            })
            .AddTimeout(TimeSpan.FromSeconds(5))
            .Build();
    }

    /// <summary>
    /// Creates a resilience pipeline for Cassandra operations.
    /// Includes retry with exponential backoff and circuit breaker.
    /// </summary>
    public static ResiliencePipeline<T> CreateCassandraPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Cassandra operation failed, attempt {Attempt} of {MaxAttempts}. Retrying after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        3,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                },
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<Cassandra.NoHostAvailableException>()
                    .Handle<Cassandra.OperationTimedOutException>()
                    .Handle<Cassandra.ReadTimeoutException>()
                    .Handle<Cassandra.WriteTimeoutException>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = FeedConstants.CIRCUIT_BREAKER_FAILURE_THRESHOLD,
                MinimumThroughput = FeedConstants.CIRCUIT_BREAKER_MIN_THROUGHPUT,
                BreakDuration = TimeSpan.FromSeconds(FeedConstants.CIRCUIT_BREAKER_BREAK_DURATION_SECONDS),
                OnOpened = args =>
                {
                    logger.LogError(
                        "Cassandra circuit breaker opened. Breaking for {BreakDuration}s. Exception: {Exception}",
                        FeedConstants.CIRCUIT_BREAKER_BREAK_DURATION_SECONDS,
                        args.Outcome.Exception?.Message);
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Cassandra circuit breaker closed. Normal operations resumed");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Cassandra circuit breaker half-open. Testing connection");
                    return default;
                },
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<Cassandra.NoHostAvailableException>()
                    .Handle<Cassandra.OperationTimedOutException>()
            })
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    /// <summary>
    /// Creates a simple retry policy without circuit breaker.
    /// Used for operations where circuit breaker is not appropriate.
    /// </summary>
    public static ResiliencePipeline<T> CreateRetryOnlyPipeline<T>(ILogger logger, int maxAttempts = 3)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = maxAttempts,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(100),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Operation failed, attempt {Attempt} of {MaxAttempts}. Retrying after {Delay}ms",
                        args.AttemptNumber,
                        maxAttempts,
                        args.RetryDelay.TotalMilliseconds);
                    return default;
                }
            })
            .Build();
    }
}
