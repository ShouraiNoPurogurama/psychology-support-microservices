using Feed.Domain.FeedConfiguration;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class FeedConfigMapper
{
    public static FeedConfigRow ToRow(FeedConfig domain) => new()
    {
        Key = domain.Key,
        Value = domain.Value
    };

    public static FeedConfig ToDomain(FeedConfigRow row)
        => FeedConfig.Create(row.Key, row.Value);
}
