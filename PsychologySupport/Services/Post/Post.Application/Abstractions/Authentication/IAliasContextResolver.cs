namespace Post.Application.Abstractions.Authentication;

public interface IAliasContextResolver
{
    Task<Guid> GetCurrentAliasVersionIdAsync(CancellationToken cancellationToken = default);
}