using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByTag;

public record GetPostsByTagQuery(
    Guid CategoryTagId,
    int Page = 1,
    int Size = 10
) : IQuery<GetPostsByTagResult>;

public record GetPostsByTagResult(
    PaginatedResult<PostSummaryDto> Posts
);

public record PostSummaryDto(
    Guid Id,
    string Content,
    string? Title,
    Guid AuthorAliasId,
    string AuthorDisplayName,
    string Visibility,
    int ReactionCount,
    int CommentCount,
    DateTimeOffset CreatedAt,
    bool HasMedia
);
