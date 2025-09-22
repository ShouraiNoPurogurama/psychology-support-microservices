using Feed.Domain.ViewerMuting;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class ViewerMutingMapper
{
    public static ViewerMutedAliasRow ToRow(ViewerMuted domain) => new()
    {
        AliasId = domain.AliasId,
        MutedAliasId = domain.MutedAliasId,
        MutedSince = domain.MutedSince
    };

    public static ViewerMuted ToDomain(ViewerMutedAliasRow row)
        => ViewerMuted.Create(
            row.AliasId,
            row.MutedAliasId,
            row.MutedSince ?? DateTimeOffset.UtcNow
        );
}
