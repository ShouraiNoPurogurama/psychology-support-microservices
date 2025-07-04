using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];

        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var requestTypeScope = LogContext.PushProperty("RequestType", requestName);
        using var responseTypeScope = LogContext.PushProperty("ResponseType", responseName);

        var timer = Stopwatch.StartNew();

        //🚀 [START]
        logger.LogInformation("🚀 [START] [{CorrelationId}] {RequestType} → {ResponseType}", correlationId, requestName, responseName);

        //📥 [REQUEST]
        if (logger.IsEnabled(LogLevel.Debug))
        {
            LogRequestDetails(request, correlationId);
        }

        try
        {
            var response = await next();
            timer.Stop();

            var duration = timer.ElapsedMilliseconds;
            using var durationScope = LogContext.PushProperty("Duration", duration);

            //[PERFORMANCE]
            if (duration > 3000)
                logger.LogWarning("⚠️ [PERFORMANCE] [{CorrelationId}] {RequestType} SLOW: {Duration}ms", correlationId, requestName, duration);
            else if (duration > 1000)
                logger.LogInformation("⏱️ [PERFORMANCE] [{CorrelationId}] {RequestType} took {Duration}ms", correlationId, requestName, duration);

            //📦 [RESPONSE]
            if (logger.IsEnabled(LogLevel.Debug))
            {
                LogResponseDetails(response, correlationId);
            }

            //✅ [COMPLETED]
            logger.LogInformation("✅ [COMPLETED] [{CorrelationId}] {RequestType} → {ResponseType} in {Duration}ms",
                correlationId, requestName, responseName, duration);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            using var durationScope = LogContext.PushProperty("Duration", timer.ElapsedMilliseconds);
            using var errorScope = LogContext.PushProperty("ErrorType", ex.GetType().Name);

            //❌ [FAILED]
            logger.LogError(ex, "❌ [FAILED] [{CorrelationId}] {RequestType} after {Duration}ms: {ErrorMessage}",
                correlationId, requestName, timer.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }

    private void LogRequestDetails(TRequest request, string correlationId)
    {
        try
        {
            var sanitizedRequest = SanitizeForLogging(request);
            var requestJson = JsonSerializer.Serialize(sanitizedRequest, _jsonOptions);
            using var requestScope = LogContext.PushProperty("RequestData", requestJson);

            logger.LogDebug("📥 [REQUEST] [{CorrelationId}] Params: {RequestJson}", correlationId, requestJson);
        }
        catch (Exception ex)
        {
            logger.LogDebug("📥 [REQUEST] [{CorrelationId}] Could not serialize request: {Error}", correlationId, ex.Message);
        }
    }

    private void LogResponseDetails(TResponse response, string correlationId)
    {
        try
        {
            var sanitizedResponse = SanitizeForLogging(response);
            var responseJson = JsonSerializer.Serialize(sanitizedResponse, _jsonOptions);
            using var responseScope = LogContext.PushProperty("ResponseData", responseJson);

            logger.LogDebug("📦 [RESPONSE] [{CorrelationId}] Result: {ResponseJson}", correlationId, responseJson);
        }
        catch (Exception ex)
        {
            logger.LogDebug("📦 [RESPONSE] [{CorrelationId}] Could not serialize response: {Error}", correlationId, ex.Message);
        }
    }

    private static object? SanitizeForLogging(object? obj)
    {
        // TODO: Implement sensitive data masking
        return obj;
    }
}