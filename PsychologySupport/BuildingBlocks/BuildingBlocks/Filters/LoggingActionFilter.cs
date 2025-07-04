using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.Filters;

public class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false, // Compact cho production
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        var actionMethod = context.RouteData.Values["action"]?.ToString();
        var httpMethod = context.HttpContext.Request.Method;

        //Push context
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var actionScope = LogContext.PushProperty("Action", actionName);
        using var controllerScope = LogContext.PushProperty("Controller", controllerName);
        using var methodScope = LogContext.PushProperty("HttpMethod", httpMethod);

        var sw = Stopwatch.StartNew();

        //[START]
        _logger.LogInformation("🚀 [START] [{CorrelationId}] {Controller}.{Action} {HttpMethod}", correlationId, controllerName,
            actionMethod, httpMethod);

        //[REQUEST]
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            LogRequestDetails(context.ActionArguments, correlationId);
        }

        try
        {
            var resultContext = await next();
            sw.Stop();

            var durationMs = sw.ElapsedMilliseconds;
            var statusCode = context.HttpContext.Response.StatusCode;

            using var durationScope = LogContext.PushProperty("Duration", durationMs);
            using var statusScope = LogContext.PushProperty("StatusCode", statusCode);

            //[RESPONSE]
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                LogResponseDetails(resultContext, correlationId);
            }

            //[PERFORMANCE] hoặc [COMPLETED]
            if (durationMs > 3000)
                _logger.LogWarning("⚠️ [PERFORMANCE] [{CorrelationId}] {Controller}.{Action} SLOW: {Duration}ms", correlationId,
                    controllerName, actionMethod, durationMs);
            else if (durationMs > 1000)
                _logger.LogInformation("⏱️ [PERFORMANCE] [{CorrelationId}] {Controller}.{Action} took {Duration}ms",
                    correlationId, controllerName, actionMethod, durationMs);

            //[COMPLETED]
            _logger.LogInformation("✅ [COMPLETED] [{CorrelationId}] {Controller}.{Action} {HttpMethod} - {StatusCode}",
                correlationId, controllerName, actionMethod, httpMethod, statusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            using var durationScope = LogContext.PushProperty("Duration", sw.ElapsedMilliseconds);
            using var errorScope = LogContext.PushProperty("ErrorType", ex.GetType().Name);

            // [FAILED]
            _logger.LogError(ex,
                "❌ [FAILED] [{CorrelationId}] {Controller}.{Action} {HttpMethod} after {Duration}ms: {ErrorMessage}",
                correlationId, controllerName, actionMethod, httpMethod, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    private void LogRequestDetails(IDictionary<string, object?> actionArguments, string correlationId)
    {
        try
        {
            var sanitizedArgs = SanitizeForLogging(actionArguments);
            var requestJson = JsonSerializer.Serialize(sanitizedArgs, _jsonOptions);
            using var requestScope = LogContext.PushProperty("RequestData", requestJson);
            _logger.LogDebug("📥 [REQUEST] [{CorrelationId}] Params: {RequestJson}", correlationId, requestJson);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("📥 [REQUEST] [{CorrelationId}] Could not serialize request: {Error}", correlationId, ex.Message);
        }
    }

    private void LogResponseDetails(ActionExecutedContext context, string correlationId)
    {
        if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            try
            {
                var sanitizedResponse = SanitizeForLogging(objectResult.Value);
                var responseJson = JsonSerializer.Serialize(sanitizedResponse, _jsonOptions);
                using var responseScope = LogContext.PushProperty("ResponseData", responseJson);
                _logger.LogDebug("📦 [RESPONSE] [{CorrelationId}] Result: {ResponseJson}", correlationId, responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("📦 [RESPONSE] [{CorrelationId}] Could not serialize response: {Error}", correlationId,
                    ex.Message);
            }
        }
    }

    private static object? SanitizeForLogging(object? obj)
    {
        //Có thể thêm logic để mask sensitive data
        //Ví dụ: password, token, credit card, etc.
        return obj;
    }
}