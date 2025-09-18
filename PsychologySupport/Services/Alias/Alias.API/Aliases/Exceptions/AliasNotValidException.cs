using Alias.API.Aliases.Enums;
using BuildingBlocks.Exceptions;

namespace Alias.API.Aliases.Exceptions;

public sealed class AliasTokenException : BadRequestException
{
    private const string Code = "ALIAS_TOKEN_ERROR";
    public AliasTokenFaultReason Reason { get; }
    public DateTimeOffset? ExpiresAt { get; }

    private static string DefaultMessage(AliasTokenFaultReason reason) => reason switch
    {
        AliasTokenFaultReason.Invalid  => "Token bí danh không hợp lệ.",
        AliasTokenFaultReason.Mismatch => "Token không khớp với bí danh.",
        AliasTokenFaultReason.Expired  => "Token bí danh đã hết hạn.",
        AliasTokenFaultReason.Revoked  => "Token bí danh đã bị thu hồi.",
        _ => "Token bí danh không hợp lệ."
    };

    public AliasTokenException(
        AliasTokenFaultReason reason,
        DateTimeOffset? expiresAt = null,
        string? internalDetail = null)
        : base(
            errorCode: Code,
            safeMessage: DefaultMessage(reason),
            internalDetail: internalDetail)
    {
        Reason = reason;
        ExpiresAt = expiresAt;
    }
}