using Feed.Domain.ViewerBlocking;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class ViewerBlockingMapper
{
    public static ViewerBlockedAliasRow ToRow(ViewerBlocked domain) => new()
    {
        AliasId = domain.AliasId,
        BlockedAliasId = domain.BlockedAliasId,
        BlockedSince = domain.BlockedSince
    };

    public static ViewerBlocked ToDomain(ViewerBlockedAliasRow row)
        => ViewerBlocked.Create(
            row.AliasId,
            row.BlockedAliasId,
            row.BlockedSince ?? DateTimeOffset.UtcNow
        );
}
