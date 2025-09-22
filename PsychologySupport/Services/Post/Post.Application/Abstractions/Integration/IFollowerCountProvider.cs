namespace Post.Application.Abstractions.Integration;

public interface IFollowerCountProvider
{
    Task<int> GetFollowerCountAsync(Guid authorAliasId, CancellationToken ct);
}

