namespace Alias.API.Aliases.Models.Aliases.Enums;

public enum AliasTokenFaultReason
{
    Invalid,    //chữ ký sai, không parse được, format sai
    Mismatch,   //aliasKey trong token không trùng label đang đặt
    Expired,    //hết hạn
    Revoked     //đã bị revoke server-side
}