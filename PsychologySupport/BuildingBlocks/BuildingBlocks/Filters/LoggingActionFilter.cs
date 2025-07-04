using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Filters;

public class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Action"] = actionName
        });

        _logger.LogInformation("🚀 [START] [{CorrelationId}] {Action}", correlationId, actionName);

        // Log input params (request)
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            try
            {
                var inputJson = JsonSerializer.Serialize(context.ActionArguments, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                _logger.LogDebug("📋 [{CorrelationId}] Request Details: {InputJson}", correlationId, inputJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("📋 [{CorrelationId}] Request Details: [Could not serialize: {Error}]", correlationId, ex.Message);
            }
        }

        var sw = Stopwatch.StartNew();
        ActionExecutedContext resultContext = null!;

        try
        {
            resultContext = await next();
            sw.Stop();

            var durationMs = sw.ElapsedMilliseconds;
            var durationSec = sw.Elapsed.TotalSeconds;

            // Log response object (nếu muốn, chỉ log nếu debug)
            if (_logger.IsEnabled(LogLevel.Debug) && resultContext.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
            {
                try
                {
                    var responseJson = JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    _logger.LogDebug("📦 [{CorrelationId}] Response: {ResponseJson}", correlationId, responseJson);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("📦 [{CorrelationId}] Response: [Could not serialize: {Error}]", correlationId, ex.Message);
                }
            }

            if (durationSec > 3)
                _logger.LogWarning("⚠️ [SLOW REQUEST] [{CorrelationId}] {Action} took {Duration}ms ({DurationSec:F2}s)", correlationId, actionName, durationMs, durationSec);
            else if (durationSec > 1)
                _logger.LogInformation("⏱️  [{CorrelationId}] {Action} took {Duration}ms", correlationId, actionName, durationMs);

            _logger.LogInformation("✅ [COMPLETED] [{CorrelationId}] {Action} in {Duration}ms", correlationId, actionName, durationMs);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "❌ *** [FAILED] *** [{CorrelationId}] {Action} after {Duration}ms: {ErrorMessage}",
                correlationId, actionName, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}
