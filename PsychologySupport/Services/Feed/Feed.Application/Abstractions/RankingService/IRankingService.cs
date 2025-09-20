using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Feed.Application.Abstractions.RankingService;

/// <summary>
/// Redis hash field names as enums to avoid magic strings
/// </summary>
public enum RankFieldName
{
    Score,
    Reactions,
    Comments,
    Ctr,
    UpdatedAt
}

public record PostRankData(
    double Score,
    int Reactions,
    int Comments,
    double Ctr,
    DateTimeOffset UpdatedAt
);

public record RankedPost(
    Guid PostId,
    Guid AuthorAliasId,
    sbyte RankBucket,
    long RankI64,
    DateTimeOffset CreatedAt
);

public interface IRankingService
{
    Task<IReadOnlyList<Guid>> GetTrendingPostsAsync(DateTime date, CancellationToken ct);
    Task UpdatePostRankAsync(Guid postId, PostRankData rankData, CancellationToken ct);
    Task<IReadOnlyList<RankedPost>> RankPostsAsync(IReadOnlyList<Guid> followedAliasIds, IReadOnlyList<Guid> trendingPostIds, int limit, CancellationToken ct);
    Task AddToTrendingAsync(Guid postId, double score, DateTime date, CancellationToken ct);
}
