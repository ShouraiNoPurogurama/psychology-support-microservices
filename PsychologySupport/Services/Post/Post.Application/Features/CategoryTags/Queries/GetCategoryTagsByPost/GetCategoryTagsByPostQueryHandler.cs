using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetCategoryTagsByPost;

internal sealed class GetCategoryTagsByPostQueryHandler : IQueryHandler<GetCategoryTagsByPostQuery, GetCategoryTagsByPostResult>
{
    private readonly IPostDbContext _context;

    public GetCategoryTagsByPostQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetCategoryTagsByPostResult> Handle(GetCategoryTagsByPostQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PostCategories
            .AsNoTracking()
            .Where(pc => pc.PostId == request.PostId && !pc.IsDeleted)
            .Join(_context.CategoryTags.AsNoTracking(),
                pc => pc.CategoryTagId,
                ct => ct.Id,
                (pc, ct) => new 
                {
                    Dto = new CategoryTagDto(
                        ct.Id,
                        ct.DisplayName
                    ),
                    ct.SortOrder
                })
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Dto.DisplayName)
            .Select(x => x.Dto);

        var totalCount = await query.CountAsync(cancellationToken);

        var categoryTagsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);
        
        var paginatedResult = new PaginatedResult<CategoryTagDto>(
            request.Page,
            request.Size,
            totalCount,
            categoryTagsData
        );

        return new GetCategoryTagsByPostResult(paginatedResult);
    }
}
