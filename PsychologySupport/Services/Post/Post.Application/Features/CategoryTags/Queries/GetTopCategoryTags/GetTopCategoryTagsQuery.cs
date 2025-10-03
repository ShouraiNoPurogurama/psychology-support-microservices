using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetTopCategoryTags;

public record GetTopCategoryTagsQuery(
    int Size = 10
) : IQuery<GetTopCategoryTagsResult>;

public record GetTopCategoryTagsResult(
    PaginatedResult<CategoryTagUsageDto> CategoryTags
);
