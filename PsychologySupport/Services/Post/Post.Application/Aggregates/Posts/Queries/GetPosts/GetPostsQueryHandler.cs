using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Data;
using Post.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Queries.GetPosts;

internal sealed class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, PaginatedResult<PostSummaryDto>>
{
    private readonly IPostDbContext _context;

    public GetPostsQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PostSummaryDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Visibility) && 
            Enum.TryParse<PostVisibility>(request.Visibility, true, out var visibility))
        {
            query = query.Where(p => p.Visibility == visibility);
        }

        if (request.CategoryTagIds?.Any() == true)
        {
            query = query.Where(p => p.Categories.Any(pc => request.CategoryTagIds.Contains(pc.CategoryTagId)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortDescending 
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            "reactioncount" => request.SortDescending
                ? query.OrderByDescending(p => p.Metrics.ReactionCount)
                : query.OrderBy(p => p.Metrics.ReactionCount),
            "commentcount" => request.SortDescending
                ? query.OrderByDescending(p => p.Metrics.CommentCount)
                : query.OrderBy(p => p.Metrics.CommentCount),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var posts = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Media)
            .Include(p => p.Categories)
                .ThenInclude(pc => pc.CategoryTag)
            .Select(p => new PostSummaryDto(
                p.Id,
                p.Content.Value.Length > 300 ? p.Content.Value.Substring(0, 300) + "..." : p.Content.Value,
                p.Content.Title,
                p.Visibility.ToString(),
                new AuthorSummaryDto(p.Author.AliasId, p.Author.AliasVersionId),
                p.Moderation.Status.ToString(),
                new MetricsSummaryDto(
                    p.Metrics.ReactionCount,
                    p.Metrics.CommentCount,
                    p.Metrics.ViewCount
                ),
                p.Media.Count,
                p.Categories.Select(pc => pc.CategoryTag.Code).ToList(),
                p.CreatedAt.Value
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.PageSize,
            totalCount,
            posts
        );
    }
}
