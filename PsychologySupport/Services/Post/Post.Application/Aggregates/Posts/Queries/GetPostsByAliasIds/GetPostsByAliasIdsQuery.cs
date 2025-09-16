using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByAliasIds;

public record GetPostsByAliasIdsQuery(
    IEnumerable<Guid> AliasIds,
    int Page = 1,
    int Size = 10
) : IQuery<GetPostsByAliasIdsResult>;

public record GetPostsByAliasIdsResult(
    PaginatedResult<PostSummaryDto> Posts
);

public record PostSummaryDto(
    Guid Id,
    string? Title,
    string Content,
    Guid AuthorAliasId,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    bool HasMedia
);
