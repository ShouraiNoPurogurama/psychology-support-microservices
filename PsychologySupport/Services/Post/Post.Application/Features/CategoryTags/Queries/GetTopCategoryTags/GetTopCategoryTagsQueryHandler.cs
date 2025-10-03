using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetTopCategoryTags;

internal sealed class GetTopCategoryTagsQueryHandler : IQueryHandler<GetTopCategoryTagsQuery, GetTopCategoryTagsResult>
{
    private readonly IPostDbContext _context;

    public GetTopCategoryTagsQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetTopCategoryTagsResult> Handle(GetTopCategoryTagsQuery request, CancellationToken cancellationToken)
    {
        // Get category tags with usage counts using AsNoTracking
        var categoryTagsData = await _context.CategoryTags
            .AsNoTracking()
            .GroupJoin(_context.PostCategories.Where(pc => !pc.IsDeleted),
                ct => ct.Id,
                pc => pc.CategoryTagId,
                (ct, postCategories) => new { CategoryTag = ct, PostCategories = postCategories })
            .Select(x => new CategoryTagUsageDto(
                x.CategoryTag.Id,
                x.CategoryTag.DisplayName,
                x.CategoryTag.Color,
                x.PostCategories.Count(),
                x.PostCategories.Any() ? x.PostCategories.Max(pc => pc.AssignedAt) : DateTimeOffset.MinValue
            ))
            .OrderByDescending(x => x.UsageCount)
            .ThenByDescending(x => x.LastUsed)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Create PaginatedResult (using size as both page size and total for top results)
        var paginatedResult = new PaginatedResult<CategoryTagUsageDto>(
            1, // Page 1 for top results
            request.Size,
            categoryTagsData.Count,
            categoryTagsData
        );

        return new GetTopCategoryTagsResult(paginatedResult);
    }
}
