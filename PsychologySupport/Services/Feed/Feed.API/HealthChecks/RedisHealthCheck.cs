using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Feed.API.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            // Perform a simple ping
            var pingTime = await db.PingAsync();

            if (pingTime.TotalMilliseconds < 100)
            {
                return HealthCheckResult.Healthy(
                    $"Redis is responsive. Ping: {pingTime.TotalMilliseconds:F2}ms");
            }
            else if (pingTime.TotalMilliseconds < 500)
            {
                return HealthCheckResult.Degraded(
                    $"Redis is slow. Ping: {pingTime.TotalMilliseconds:F2}ms");
            }
            else
            {
                return HealthCheckResult.Degraded(
                    $"Redis is very slow. Ping: {pingTime.TotalMilliseconds:F2}ms");
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis health check failed: Connection error");
            return HealthCheckResult.Unhealthy(
                "Redis is unreachable. Connection failed.",
                ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis health check timed out");
            return HealthCheckResult.Degraded(
                "Redis health check timed out",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed with unexpected error");
            return HealthCheckResult.Unhealthy(
                "Redis health check failed",
                ex);
        }
    }
}
