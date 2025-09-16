using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByTag;

internal sealed class GetPostsByTagQueryHandler : IQueryHandler<GetPostsByTagQuery, GetPostsByTagResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;

    public GetPostsByTagQueryHandler(IPostDbContext context, IQueryDbContext queryContext)
    {
        _context = context;
        _queryContext = queryContext;
    }

    public async Task<GetPostsByTagResult> Handle(GetPostsByTagQuery request, CancellationToken cancellationToken)
    {
        // Verify category tag exists
        var categoryExists = await _context.CategoryTags
            .AsNoTracking()
            .AnyAsync(ct => ct.Id == request.CategoryTagId, cancellationToken);

        if (!categoryExists)
            throw new NotFoundException("Category tag not found.", "CATEGORY_TAG_NOT_FOUND");

        // Get posts by category tag with pagination and AsNoTracking
        var query = _context.PostCategories
            .AsNoTracking()
            .Where(pc => pc.CategoryTagId == request.CategoryTagId && !pc.IsDeleted)
            .Join(_context.Posts,
                pc => pc.PostId,
                p => p.Id,
                (pc, p) => p)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Get posts data with pagination
        var postsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Get author display names from query context (no EF join)
        var authorIds = postsData.Select(p => p.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        // Merge in memory (following manifesto rules)
        var postDtos = postsData.Select(p =>
        {
            var author = authorAliases.FirstOrDefault(a => a.AliasId == p.Author.AliasId);
            return new PostSummaryDto(
                p.Id,
                p.Content.Title,
                p.Content.Value,
                p.Author.AliasId,
                p.Visibility,
                new DateTimeOffset(p.PublishedAt),
                p.EditedAt.HasValue ? new DateTimeOffset(p.EditedAt.Value) : null,
                p.Metrics.ReactionCount,
                p.Metrics.CommentCount,
                p.Metrics.ViewCount,
                p.HasMedia);
        }).ToList();

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postDtos
        );

        return new GetPostsByTagResult(paginatedResult);
    }
}
