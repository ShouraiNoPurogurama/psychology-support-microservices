using Feed.Domain.UserActivity;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserActivityMapper
{
    public static FeedSeenByDayRow ToRow(FeedSeenEntry domain) => new()
    {
        AliasId = domain.AliasId,
        Ymd = domain.Ymd,
        SeenAt = domain.SeenAt,
        PostId = domain.PostId
    };

    public static FeedSeenEntry ToDomain(FeedSeenByDayRow row)
        => FeedSeenEntry.Create(
            row.AliasId,
            row.PostId,
            row.Ymd,
            row.SeenAt
        );
}
