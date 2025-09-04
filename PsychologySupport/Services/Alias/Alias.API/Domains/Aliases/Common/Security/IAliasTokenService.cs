namespace Alias.API.Domains.Aliases.Common.Security;

public interface IAliasTokenService
{
    string Create(string aliasKey, DateTimeOffset expiresAt);
    bool TryValidate(string token, out string aliasKey, out DateTimeOffset expiresAt);
}