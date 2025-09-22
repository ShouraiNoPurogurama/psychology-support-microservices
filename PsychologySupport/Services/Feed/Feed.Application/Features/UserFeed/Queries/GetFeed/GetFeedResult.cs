namespace Feed.Application.Features.UserFeed.Queries.GetFeed;

public record GetFeedResult(
    IReadOnlyList<UserFeedItemDto> Items,
    string? NextCursor,
    bool HasMore,
    int TotalCount
);

public record UserFeedItemDto(
    Guid PostId,
    DateOnly YmdBucket,
    short Shard,
    sbyte RankBucket,
    long RankI64,
    Guid TsUuid,
    DateTimeOffset? CreatedAt,
    bool IsPinned = false
);
