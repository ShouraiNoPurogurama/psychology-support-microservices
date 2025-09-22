using Feed.Domain.FollowerTracking;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class FollowerTrackingMapper
{
    public static FollowersByAliasRow ToRow(Follower domain) => new()
    {
        AuthorAliasId = domain.AuthorAliasId,
        AliasId = domain.AliasId,
        Since = domain.Since
    };

    public static Follower ToDomain(FollowersByAliasRow row)
        => Follower.Create(
            row.AuthorAliasId,
            row.AliasId,
            row.Since ?? DateTimeOffset.UtcNow
        );
}
