using BuildingBlocks.Exceptions.Base;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class NotReadyException : CustomException
{
    private const string Code = "NOT_READY";
    private const string DefaultSafeMessage = "Tài nguyên chưa sẵn sàng. Vui lòng thử lại sau.";

    public NotReadyException()
        : base(errorCode: Code, statusCode: StatusCodes.Status451UnavailableForLegalReasons, 
            safeMessage: DefaultSafeMessage)
    {
    }

    public NotReadyException(string safeMessage, string errorCode = Code, string? internalDetail = null)
        : base(errorCode: errorCode, statusCode: StatusCodes.Status451UnavailableForLegalReasons, 
            safeMessage: safeMessage,
            internalDetail: internalDetail)
    {
    }
}