using FluentValidation;
using BuildingBlocks.Exceptions.Base; // Nơi chứa CustomException và ValidationException của bạn
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Lỗi hệ thống";
        var detail = "Đã có lỗi không mong muốn xảy ra. Vui lòng thử lại sau.";
        var errorCode = "INTERNAL_SERVER_ERROR";
        IReadOnlyDictionary<string, string[]>? validationErrors = null;

        switch (exception)
        {
            case CustomValidationException cve: //Custom validation exception
                statusCode = StatusCodes.Status400BadRequest;
                title = "Lỗi xác thực";
                detail = "Một hoặc nhiều trường dữ liệu không hợp lệ.";
                errorCode = cve.ErrorCode ?? "VALIDATION_ERROR";
                validationErrors = cve.Errors;
                logger.LogWarning(exception, "Custom ValidationException caught: {ErrorCode}", errorCode);
                break;

            case FluentValidation.ValidationException fve:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Lỗi xác thực";
                detail = "Một hoặc nhiều trường dữ liệu không hợp lệ.";
                errorCode = "VALIDATION_ERROR";
                validationErrors = fve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                logger.LogWarning(exception, "FluentValidation.ValidationException caught.");
                break;
            
            case CustomException ce:
                statusCode = ce.StatusCode;
                title = ce.GetType().Name.Replace("Exception", ""); // "NotFound", "BadRequest"
                detail = ce.SafeMessage;
                errorCode = ce.ErrorCode;
                logger.LogWarning(exception, "CustomException caught: {ErrorCode}, Detail: {InternalDetail}", ce.ErrorCode, ce.InternalDetail);
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Không có quyền truy cập";
                detail = "Bạn chưa được xác thực hoặc phiên đăng nhập đã hết hạn.";
                errorCode = "UNAUTHORIZED";
                logger.LogWarning(exception, "UnauthorizedAccessException caught.");
                break;

            default:
                logger.LogError(exception, "Unhandled exception caught.");
                break;
        }

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (validationErrors is not null)
        {
            problemDetails.Extensions["errors"] = validationErrors;
        }

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}