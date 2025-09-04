using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class ConflictException : CustomException
{
    private const string Code = "CONFLICT";
    private const string DefaultSafeMessage = "Dữ liệu đã tồn tại.";

    public ConflictException()
        : base(errorCode: Code, statusCode: StatusCodes.Status409Conflict, safeMessage: DefaultSafeMessage)
    {
    }

    public ConflictException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status409Conflict, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }
}