using Feed.Domain.UserActivity;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserActivityMapper
{
    public static FeedSeenByDayRow ToRow(FeedSeenEntry domain) => new()
    {
        AliasId = domain.AliasId,
        Ymd = CassandraTypeMapper.ToLocalDate(domain.Ymd),
        SeenAt = CassandraTypeMapper.ToTimeUuid(domain.SeenAt),
        PostId = domain.PostId
    };

    public static FeedSeenEntry ToDomain(FeedSeenByDayRow row)
        => FeedSeenEntry.Create(
            row.AliasId,
            row.PostId,
            CassandraTypeMapper.ToDateOnly(row.Ymd),
            CassandraTypeMapper.ToGuid(row.SeenAt)
        );
}
