using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Post.Application.Aggregates.CategoryTags.Queries.GetCategoryTagsByPost;

public record GetCategoryTagsByPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetCategoryTagsByPostResult>;

public record GetCategoryTagsByPostResult(
    PaginatedResult<CategoryTagDto> CategoryTags
);