using BuildingBlocks.CQRS;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetAllCategoryTags;

public record GetAllCategoryTagsQuery(
    bool? IsActive = null
) : IQuery<GetAllCategoryTagsResult>;

public record GetAllCategoryTagsResult(
    IEnumerable<CategoryTagDetailDto> CategoryTags
);
