using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class InternalServerException : CustomException
{
    private const string Code = "INTERNAL_SERVER_ERROR";
    private const string DefaultSafeMessage = "Đã có lỗi xảy ra từ hệ thống, vui lòng liên hệ fanpage của ứng dụng hoặc thử lại sau.";

    public InternalServerException()
        : base(errorCode: Code, statusCode: StatusCodes.Status500InternalServerError, safeMessage: DefaultSafeMessage)
    {
    }

    public InternalServerException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status500InternalServerError, safeMessage: safeMessage, internalDetail: internalDetail)
    {
    }
    
}