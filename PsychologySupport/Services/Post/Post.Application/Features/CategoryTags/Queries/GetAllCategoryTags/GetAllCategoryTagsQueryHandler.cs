using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.CategoryTags.Dtos;

namespace Post.Application.Features.CategoryTags.Queries.GetAllCategoryTags;

internal sealed class GetAllCategoryTagsQueryHandler : IQueryHandler<GetAllCategoryTagsQuery, GetAllCategoryTagsResult>
{
    private readonly IPostDbContext _context;

    public GetAllCategoryTagsQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllCategoryTagsResult> Handle(GetAllCategoryTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CategoryTags.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(ct => ct.IsActive == request.IsActive.Value);
        }

        var categoryTags = await query
            .OrderBy(ct => ct.SortOrder)
            .ThenBy(ct => ct.DisplayName)
            .Select(ct => new CategoryTagDetailDto(
                ct.Id,
                ct.Code,
                ct.DisplayName,
                ct.Color,
                ct.UnicodeCodepoint,
                ct.IsActive,
                ct.SortOrder
            ))
            .ToListAsync(cancellationToken);

        return new GetAllCategoryTagsResult(categoryTags);
    }
}
