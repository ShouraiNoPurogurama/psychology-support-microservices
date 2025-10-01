using System.Diagnostics;
using Serilog.Context;

namespace YarpApiGateway.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activityId = Activity.Current?.Id;
        var correlationId = activityId != null
            ? activityId.Split('-').ElementAtOrDefault(1)?[..8] ?? Guid.NewGuid().ToString("N")[..8]
            : Guid.NewGuid().ToString("N")[..8];

        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;
        var startTime = DateTimeOffset.Now.ToString("HH:mm:ss");

        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var pathScope = LogContext.PushProperty("Path", path);

        var timer = Stopwatch.StartNew();

        _logger.LogInformation("[{Time}] 🚀 [START] [{CorrelationId}] {Method} {Path}", startTime, correlationId, method, path);

        try
        {
            await _next(context);
            timer.Stop();

            var duration = timer.Elapsed.TotalMilliseconds;
            var statusCode = context.Response.StatusCode;

            using var durationScope = LogContext.PushProperty("Duration", duration);
            using var statusScope = LogContext.PushProperty("StatusCode", statusCode);

            var endTime = DateTimeOffset.Now.ToString("HH:mm:ss");

            if (duration > 3000)
            {
                _logger.LogWarning("[{Time}] ⚠️ [SLOW] [{CorrelationId}] {Method} {Path} - {StatusCode} {Duration:0.##}ms",
                    endTime, correlationId, method, path, statusCode, duration);
            }

            _logger.LogInformation("[{Time}] ✅ [COMPLETED] [{CorrelationId}] {Method} {Path} - {StatusCode} {Duration:0.##}ms",
                endTime, correlationId, method, path, statusCode, duration);
        }
        catch (Exception ex)
        {
            timer.Stop();
            var duration = timer.Elapsed.TotalMilliseconds;
            var errorTime = DateTimeOffset.Now.ToString("HH:mm:ss");

            _logger.LogError(ex,
                "[{Time}] ❌ [FAILED] [{CorrelationId}] {Method} {Path} after {Duration:0.##}ms: {ErrorMessage}",
                errorTime, correlationId, method, path, duration, ex.Message);

            throw;
        }
    }
}
