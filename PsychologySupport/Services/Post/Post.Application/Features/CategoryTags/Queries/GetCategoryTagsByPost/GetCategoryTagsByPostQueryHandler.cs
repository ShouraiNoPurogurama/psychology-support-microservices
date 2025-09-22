using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
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
        // Verify post exists
        var postExists = await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (!postExists)
            throw new NotFoundException("Post not found or has been deleted.", "POST_NOT_FOUND");

        // Query category tags with pagination and AsNoTracking
        var query = _context.PostCategories
            .AsNoTracking()
            .Where(pc => pc.PostId == request.PostId && !pc.IsDeleted)
            .Join(_context.CategoryTags,
                pc => pc.CategoryTagId,
                ct => ct.Id,
                (pc, ct) => new CategoryTagDto(
                    ct.Id,
                    ct.DisplayName,
                    ct.Color,
                    pc.AssignedAt
                ))
            .OrderBy(ct => ct.Name);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply Skip/Take for pagination
        var categoryTagsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Create complete PaginatedResult object
        var paginatedResult = new PaginatedResult<CategoryTagDto>(
            request.Page,
            request.Size,
            totalCount,
            categoryTagsData
        );

        return new GetCategoryTagsByPostResult(paginatedResult);
    }
}
