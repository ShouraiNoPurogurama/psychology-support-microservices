using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPosts;

public record GetPostsQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Visibility = null,
    List<Guid>? CategoryTagIds = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IQuery<PaginatedResult<PostSummaryDto>>;

public record AuthorSummaryDto(
    Guid AliasId,
    Guid? AliasVersionId
);

public record MetricsSummaryDto(
    int ReactionCount,
    int CommentCount,
    int ViewCount
);
