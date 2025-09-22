using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetPostsByCategoryTag;

public record GetPostsByCategoryTagQuery(
    Guid CategoryTagId,
    int Page = 1,
    int Size = 10
) : IQuery<GetPostsByCategoryTagResult>;

public record GetPostsByCategoryTagResult(
    PaginatedResult<PostSummaryDto> Posts
);