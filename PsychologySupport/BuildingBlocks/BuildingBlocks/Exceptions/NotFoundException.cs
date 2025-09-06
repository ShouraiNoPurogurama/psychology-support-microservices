using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class NotFoundException : CustomException
{
    private const string Code = "NOT_FOUND";
    private const string DefaultSafeMessage = "Không tìm thấy tài nguyên.";

    public NotFoundException()
        : base(errorCode: Code, statusCode: StatusCodes.Status404NotFound, safeMessage: DefaultSafeMessage)
    {
    }

    public NotFoundException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status404NotFound, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }

    [Obsolete("Dùng constructor (string safeMessage, string? internalDetail = null) thay thế để tránh leak domain logic")]
    public NotFoundException(string entityName, object key)
        : base(errorCode: Code, statusCode: StatusCodes.Status404NotFound, safeMessage: DefaultSafeMessage, internalDetail: $"{entityName}:{key}")
    {
    }
}