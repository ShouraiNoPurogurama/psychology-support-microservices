using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetCategoryTagsByPost;

public record GetCategoryTagsByPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetCategoryTagsByPostResult>;

public record GetCategoryTagsByPostResult(
    PaginatedResult<CategoryTagDto> CategoryTags
);