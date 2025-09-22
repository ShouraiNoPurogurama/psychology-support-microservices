using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByAliasIds;

internal sealed class GetPostsByAliasIdsQueryHandler : IQueryHandler<GetPostsByAliasIdsQuery, GetPostsByAliasIdsResult>
{
    private readonly IPostDbContext _context;

    public GetPostsByAliasIdsQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetPostsByAliasIdsResult> Handle(GetPostsByAliasIdsQuery request, CancellationToken cancellationToken)
    {
        // Query posts by alias IDs with pagination and AsNoTracking
        var query = _context.Posts
            .AsNoTracking()
            .Where(p => request.AliasIds.Contains(p.Author.AliasId) && 
                       !p.IsDeleted && 
                       p.Visibility == PostVisibility.Public)
            .Select(p => new PostSummaryDto(
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
                p.HasMedia
            ))
            .OrderByDescending(p => p.PublishedAt);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply Skip/Take for pagination
        var postsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Create complete PaginatedResult object
        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postsData
        );

        return new GetPostsByAliasIdsResult(paginatedResult);
    }
}