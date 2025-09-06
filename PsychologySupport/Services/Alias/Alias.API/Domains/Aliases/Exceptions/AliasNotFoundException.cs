using BuildingBlocks.Exceptions;

namespace Alias.API.Domains.Aliases.Exceptions;

public class AliasNotFoundException : NotFoundException
{
    private const string Code = "ALIAS_NOT_FOUND";
    private const string DefaultSafeMessage = "Không tìm thấy hồ sơ ẩn danh của người dùng.";

    public AliasNotFoundException()
        : base(
            errorCode: Code,
            safeMessage: DefaultSafeMessage,
            internalDetail: null
        )
    {
    }

    public AliasNotFoundException(string safeMessage, string? internalDetail = null)
        : base(
            errorCode: Code,
            safeMessage: safeMessage,
            internalDetail: internalDetail
        )
    {
    }
}