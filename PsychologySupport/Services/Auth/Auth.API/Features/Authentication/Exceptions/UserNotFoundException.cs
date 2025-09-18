using BuildingBlocks.Exceptions;

namespace Auth.API.Features.Authentication.Exceptions;

public class UserNotFoundException : NotFoundException
{
    private const string Code = "USER_NOT_FOUND";
    private const string DefaultSafeMessage = "Không tìm thấy người dùng.";

    public UserNotFoundException()
        : base(
            errorCode: Code,
            safeMessage: DefaultSafeMessage,
            internalDetail: null
        )
    {
    }

    public UserNotFoundException(string safeMessage, string? internalDetail = null)
        : base(
            errorCode: Code,
            safeMessage: safeMessage,
            internalDetail: internalDetail
        )
    {
    }
}