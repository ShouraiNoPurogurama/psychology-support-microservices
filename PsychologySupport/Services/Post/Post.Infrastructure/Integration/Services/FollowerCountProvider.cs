using Post.Application.Abstractions.Integration;

namespace Post.Infrastructure.Integration.Services;

public sealed class FollowerCountProvider : IFollowerCountProvider
{
    public Task<int> GetFollowerCountAsync(Guid authorAliasId, CancellationToken ct)
    {
        // TODO: Call Alias service or cache to fetch real follower count.
        return Task.FromResult(0);
    }
}

