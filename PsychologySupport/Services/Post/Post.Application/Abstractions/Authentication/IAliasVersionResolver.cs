namespace Post.Application.Abstractions.Authentication;

public interface IAliasVersionResolver
{
    Task<Guid> GetCurrentAliasVersionIdAsync(CancellationToken cancellationToken = default);
}
