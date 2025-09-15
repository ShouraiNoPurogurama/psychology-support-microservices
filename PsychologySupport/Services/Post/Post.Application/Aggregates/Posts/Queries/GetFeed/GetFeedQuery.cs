using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Posts.Queries.GetFeed;

public record GetFeedQuery(
    int PageNumber,
    int PageSize
) : IQuery<PaginatedResult<PostDto>>;
