using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByTag;

public record GetPostsByTagQuery(
    Guid CategoryTagId,
    int Page = 1,
    int Size = 10
) : IQuery<GetPostsByTagResult>;

public record GetPostsByTagResult(
    PaginatedResult<PostSummaryDto> Posts
);
