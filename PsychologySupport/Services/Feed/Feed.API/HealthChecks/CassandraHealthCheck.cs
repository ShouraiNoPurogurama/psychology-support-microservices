using Cassandra;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ISession = Cassandra.ISession;


namespace Feed.API.HealthChecks;

/// <summary>
/// Health check for Cassandra database connectivity.
/// </summary>
public class CassandraHealthCheck : IHealthCheck
{
    private readonly ISession _session;
    private readonly ILogger<CassandraHealthCheck> _logger;

    public CassandraHealthCheck(ISession session, ILogger<CassandraHealthCheck> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple query to check connectivity
            var rs = await _session.ExecuteAsync(
                new SimpleStatement("SELECT now() FROM system.local")
                    .SetReadTimeoutMillis(5000));

            if (rs.Any())
            {
                var timestamp = rs.First().GetValue<DateTimeOffset>(0);
                return HealthCheckResult.Healthy(
                    $"Cassandra is responsive. Server time: {timestamp:u}");
            }

            return HealthCheckResult.Degraded("Cassandra returned no results");
        }
        catch (NoHostAvailableException ex)
        {
            _logger.LogError(ex, "Cassandra health check failed: No hosts available");
            return HealthCheckResult.Unhealthy(
                "Cassandra cluster is unreachable. No hosts available.",
                ex);
        }
        catch (OperationTimedOutException ex)
        {
            _logger.LogWarning(ex, "Cassandra health check timed out");
            return HealthCheckResult.Degraded(
                "Cassandra is slow to respond (timeout)",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cassandra health check failed with unexpected error");
            return HealthCheckResult.Unhealthy(
                "Cassandra health check failed",
                ex);
        }
    }
}
