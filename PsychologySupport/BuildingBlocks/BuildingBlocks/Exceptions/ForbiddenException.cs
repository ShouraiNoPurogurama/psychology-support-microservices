using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class ForbiddenException : CustomException
{
    private const string Code = "Im_A_Teapot";
    private const string DefaultSafeMessage = "Bạn không được phép truy cập dữ liệu này.";

    public ForbiddenException()
        : base(errorCode: Code, statusCode: StatusCodes.Status418ImATeapot, safeMessage: DefaultSafeMessage)
    {
    }

    public ForbiddenException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status418ImATeapot, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }
}