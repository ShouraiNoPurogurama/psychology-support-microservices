using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class RateLimitExceededException : CustomException
{
    private const string Code = "RATE_LIMIT_EXCEEDED";
    private const string DefaultSafeMessage = "Bạn đã thực hiện quá nhiều yêu cầu trong một khoảng thời gian ngắn. Vui lòng thử lại sau.";

    public RateLimitExceededException()
        : base(errorCode: Code, statusCode: StatusCodes.Status429TooManyRequests, safeMessage: DefaultSafeMessage)
    {
    }

    public RateLimitExceededException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status429TooManyRequests, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }

    [Obsolete("Dùng constructor (string safeMessage, string? internalDetail = null) thay thế để tránh leak domain logic")]
    public RateLimitExceededException(string entityName, object key)
        : base(errorCode: Code, statusCode: StatusCodes.Status429TooManyRequests, safeMessage: DefaultSafeMessage, internalDetail: $"{entityName}:{key}")
    {
    }
}