namespace Alias.API.Common.Authentication;

public interface IAliasVersionAccessor
{
    Task<(bool ok, Guid aliasVersionId)> TryGetCurrentAliasVersionIdAsync(CancellationToken ct = default);
    Task<Guid> GetRequiredCurrentAliasVersionIdAsync(CancellationToken ct = default); // throws if missing
}
