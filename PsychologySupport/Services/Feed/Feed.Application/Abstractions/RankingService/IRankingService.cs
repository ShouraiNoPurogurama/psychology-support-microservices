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
    DateTimeOffset UpdatedAt,
    DateTimeOffset CreatedAt,
    Guid AuthorAliasId
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

    Task<IReadOnlyList<RankedPost>> RankPostsAsync(IReadOnlyList<Guid> followedAliasIds, IReadOnlyList<Guid> trendingPostIds,
        int limit, CancellationToken ct);

    Task AddToTrendingAsync(Guid postId, double score, DateTime date, CancellationToken ct);



    // Helpers to support background rank updates and filtering
    Task InitializePostRankAsync(Guid postId, DateTimeOffset createdAt, CancellationToken ct);
    Task<PostRankData?> GetPostRankAsync(Guid postId, CancellationToken ct);
    Task IncrementReactionsAsync(Guid postId, int delta, CancellationToken ct);
    Task IncrementCommentsAsync(Guid postId, int delta, CancellationToken ct);

    // PostId -> AuthorAliasId mapping for filtering
    Task SetPostAuthorAsync(Guid postId, Guid authorAliasId, CancellationToken ct);
    Task<Guid?> GetPostAuthorAsync(Guid postId, CancellationToken ct);
}