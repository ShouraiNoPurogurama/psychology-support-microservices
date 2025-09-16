using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Post.Application.Aggregates.CategoryTags.Queries.GetTopCategoryTags;

public record GetTopCategoryTagsQuery(
    int Size = 10
) : IQuery<GetTopCategoryTagsResult>;

public record GetTopCategoryTagsResult(
    PaginatedResult<CategoryTagUsageDto> CategoryTags
);

public record CategoryTagUsageDto(
    Guid Id,
    string DisplayName,
    string Color,
    int UsageCount,
    DateTimeOffset LastUsed
);
