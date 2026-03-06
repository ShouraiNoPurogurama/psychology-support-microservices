using Feed.Domain.PostReplica;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

/// <summary>
/// Mapper for PostEngagement domain models to/from Cassandra row models.
/// </summary>
public static class PostEngagementMapper
{
    // PostEngagementMetadata mappings
    public static PostEngagementMetadataRow ToRow(PostEngagementMetadata domain) => new()
    {
        PostId = domain.PostId,
        AuthorAliasId = domain.AuthorAliasId,
        CreatedAt = domain.CreatedAt,
        CountersLastUpdated = domain.CountersLastUpdated
    };

    public static PostEngagementMetadata ToDomain(PostEngagementMetadataRow row)
        => PostEngagementMetadata.Create(
            row.PostId,
            row.AuthorAliasId,
            row.CreatedAt,
            row.CountersLastUpdated
        );

    // PostEngagementCounters mappings
    public static PostEngagementCountersRow ToRow(PostEngagementCounters domain) => new()
    {
        PostId = domain.PostId,
        Reactions = domain.Reactions,
        Comments = domain.Comments,
        Shares = domain.Shares,
        Clicks = domain.Clicks,
        Impressions = domain.Impressions,
        ViewDurationSec = domain.ViewDurationSec
    };

    public static PostEngagementCounters ToDomain(PostEngagementCountersRow row)
        => PostEngagementCounters.Create(
            row.PostId,
            row.Reactions ?? 0,
            row.Comments ?? 0,
            row.Shares ?? 0,
            row.Clicks ?? 0,
            row.Impressions ?? 0,
            row.ViewDurationSec ?? 0
        );
}
