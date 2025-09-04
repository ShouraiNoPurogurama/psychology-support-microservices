using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class BadRequestException : CustomException
{
    
    private const string Code = "BAD_REQUEST";
    private const string DefaultSafeMessage = "Yêu cầu không hợp lệ.";

    public BadRequestException()
        : base(errorCode: Code, statusCode: StatusCodes.Status400BadRequest, safeMessage: DefaultSafeMessage)
    {
    }

    public BadRequestException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status400BadRequest, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }
}