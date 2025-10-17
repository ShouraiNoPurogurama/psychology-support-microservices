using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Reactions.Queries.GetReactedPosts;

public record GetReactedPostsQuery(
    Guid AliasId,
    int PageIndex = 1,
    int PageSize = 20,
    string? ReactionCode = null,
    bool SortDescending = true
) : IQuery<GetReactedPostsResult>;

public record GetReactedPostsResult(
    PaginatedResult<PostSummaryDto> Posts
);
