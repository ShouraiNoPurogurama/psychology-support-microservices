using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Exceptions.Base;

namespace BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = exception.GetType().Name;
        var detail = "Đã có lỗi xảy ra từ hệ thống, vui lòng liên hệ fanpage của ứng dụng hoặc thử lại sau.";
        string errorCode = "INTERNAL_SERVER_ERROR";

        switch (exception)
        {
            case CustomException ce:
            {
                statusCode = ce.StatusCode;
                detail = ce.SafeMessage;
                errorCode = ce.ErrorCode;

                //không lộ internal detail cho client
                logger.LogError(exception,
                    "CustomException caught. Code={ErrorCode}, Status={Status}, Message={SafeMessage}, InternalDetail={InternalDetail}, TraceId={TraceId}",
                    ce.ErrorCode, ce.StatusCode, ce.SafeMessage, ce.InternalDetail, context.TraceIdentifier);
                break;
            }

            //FluentValidation
            case ValidationException ve:
            {
                statusCode = StatusCodes.Status400BadRequest;
                errorCode = "VALIDATION_ERROR";
                detail = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";

                logger.LogWarning(exception,
                    "ValidationException caught. TraceId={TraceId}",
                    context.TraceIdentifier);

                var problem = new ProblemDetails
                {
                    Title = "ValidationException",
                    Detail = detail,
                    Status = statusCode,
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;
                problem.Extensions["errorCode"] = errorCode;

                var validationErrors = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                problem.Extensions["ValidationErrors"] = validationErrors;

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(problem, cancellationToken);
                return true;
            }

            //Một số exception .NET phổ biến ngoài hệ CustomException (fallback)
            case UnauthorizedAccessException:
            {
                statusCode = StatusCodes.Status401Unauthorized;
                errorCode = "UNAUTHORIZED";
                detail = "Bạn chưa được xác thực hoặc phiên đăng nhập đã hết hạn.";

                logger.LogWarning(exception,
                    "UnauthorizedAccessException caught. TraceId={TraceId}",
                    context.TraceIdentifier);
                break;
            }
            case OperationCanceledException:
            {
                statusCode = StatusCodes
                    .Status499ClientClosedRequest; // gần nghĩa client hủy (Nginx semantics); có thể dùng 400 nếu muốn an toàn.
                errorCode = "OPERATION_CANCELED";
                detail = "Yêu cầu đã bị hủy.";

                logger.LogInformation("OperationCanceledException. TraceId={TraceId}", context.TraceIdentifier);
                break;
            }

            default:
            {
                //Unknown -> 500
                logger.LogError(exception,
                    "Unhandled exception. TraceId={TraceId}",
                    context.TraceIdentifier);
                break;
            }
        }

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = errorCode;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}


// using FluentValidation;
// using Microsoft.AspNetCore.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
//
// namespace BuildingBlocks.Exceptions.Handler;
//
// public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
// {
//     public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
//     {
//         //1. Log error
//         //2. Create a details object based on the occuring Exception
//         //3. Initialize a ProblemDetails object based on the data of the created detail object
//         //4. Add traceId to the problemDetails obj extension
//         //5. If this is ValidationException, add ValidationErrors to problemDetails obj extension
//         //6. Write the problemDetails obj into response as json
//
//         logger.LogError("Error Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);
//         
//         
//         (string detail, string title, int statusCode) details = exception switch
//         {
//             InternalServerException =>
//             (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status500InternalServerError
//             ),
//             BadRequestException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status400BadRequest
//             ),
//             NotFoundException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status404NotFound
//             ),
//             ValidationException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status400BadRequest
//             ),
//             ForbiddenException => (
//                 exception.Message,
//                 // exception.GetType().Name,
//                 "ImATeapot",
//                 context.Response.StatusCode = StatusCodes.Status418ImATeapot //Using 418 as a placeholder for Forbidden
//             ),
//             UnauthorizedAccessException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status401Unauthorized
//             ),
//             RateLimitExceededException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status429TooManyRequests
//             ),
//             ConflictException => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status409Conflict
//                 ),
//             _ => (
//                 exception.Message,
//                 exception.GetType().Name,
//                 context.Response.StatusCode = StatusCodes.Status500InternalServerError
//             )
//         };
//
//         var problemDetails = new ProblemDetails
//         {
//             Title = details.title,
//             Detail = details.detail,
//             Status = details.statusCode,
//             Instance = context.Request.Path
//         };
//
//         problemDetails.Extensions.Add("traceId", context.TraceIdentifier);
//
//         if (exception is ValidationException validationException)
//         {
//             problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
//         }
//
//         await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
//         return true;
//     }
// }