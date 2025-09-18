using BuildingBlocks.Exceptions;

namespace Alias.API.Aliases.Exceptions;

public sealed class AliasConflictException : ConflictException
{
    private const string Code = "ALIAS_CONFLICT";
    private const string DefaultSafeMessage = "Bí danh đã tồn tại.";

    public AliasConflictException()
        : base(
            errorCode: Code,
            safeMessage: DefaultSafeMessage,
            internalDetail: null
        )
    {
    }

    public AliasConflictException(string safeMessage, string? internalDetail = null)
        : base(
            errorCode: Code,
            safeMessage: safeMessage,
            internalDetail: internalDetail
        )
    {
    }
}