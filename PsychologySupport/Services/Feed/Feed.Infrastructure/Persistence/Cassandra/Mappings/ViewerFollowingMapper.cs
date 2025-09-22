using Feed.Domain.ViewerFollowing;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class ViewerFollowingMapper
{
    public static FollowsByViewerRow ToRow(ViewerFollow domain) => new()
    {
        AliasId = domain.AliasId,
        FollowedAliasId = domain.FollowedAliasId,
        Since = domain.Since
    };

    public static ViewerFollow ToDomain(FollowsByViewerRow row)
        => ViewerFollow.Create(
            row.AliasId,
            row.FollowedAliasId,
            row.Since ?? DateTimeOffset.UtcNow
        );
}
