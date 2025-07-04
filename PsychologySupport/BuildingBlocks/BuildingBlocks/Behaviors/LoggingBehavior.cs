using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;
        var correlationId = Guid.NewGuid().ToString("N")[..8]; //Short correlation ID

        //Start log - more structured
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestType"] = requestName,
            ["ResponseType"] = responseName
        });

        logger.LogInformation("🚀 [{CorrelationId}] Starting {RequestType}", correlationId, requestName);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            try
            {
                var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                logger.LogDebug("📋 [{CorrelationId}] Request Details: {RequestDetails}", correlationId, requestJson);
            }
            catch (Exception ex)
            {
                logger.LogDebug("📋 [{CorrelationId}] Request Details: [Could not serialize: {Error}]", correlationId, ex.Message);
            }
        }

        var timer = Stopwatch.StartNew();
        TResponse response;
        
        try
        {
            response = await next();
            timer.Stop();

            var duration = timer.ElapsedMilliseconds;
            var durationSeconds = timer.Elapsed.TotalSeconds;

            //Performance warning
            if (durationSeconds > 3)
            {
                logger.LogWarning("⚠️  [{CorrelationId}] SLOW REQUEST: {RequestType} took {Duration}ms ({DurationSeconds:F2}s)", 
                    correlationId, requestName, duration, durationSeconds);
            }
            else if (durationSeconds > 1)
            {
                logger.LogInformation("⏱️  [{CorrelationId}] {RequestType} took {Duration}ms", 
                    correlationId, requestName, duration);
            }

            logger.LogInformation("✅ [{CorrelationId}] Completed {RequestType} → {ResponseType} in {Duration}ms", 
                correlationId, requestName, responseName, duration);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            logger.LogError(ex, "❌ [{CorrelationId}] Failed {RequestType} after {Duration}ms: {ErrorMessage}", 
                correlationId, requestName, timer.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}