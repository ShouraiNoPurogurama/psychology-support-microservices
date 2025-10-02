using Feed.Domain.FollowerTracking;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class FollowerTrackingMapper
{
    public static FollowersOfAliasRow ToRow(Follower domain) => new()
    {
        FollowerAliasId = domain.FollowerAliasId,
        AliasId = domain.AliasId,
        Since = domain.Since
    };

    public static Follower ToDomain(FollowersOfAliasRow row)
        => Follower.Create(
            row.FollowerAliasId,
            row.AliasId,
            row.Since ?? DateTimeOffset.UtcNow
        );
}
