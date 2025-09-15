using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Data;
using Post.Application.Aggregates.Posts.Dtos;
using Post.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Queries.GetFeed;

internal sealed class GetFeedQueryHandler : IQueryHandler<GetFeedQuery, PaginatedResult<PostDto>>
{
    private readonly IPostDbContext _context;

    public GetFeedQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PostDto>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .Where(p => p.IsPublished && !p.IsDeleted && p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.PublishedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostDto(
                p.Id,
                p.Content.Value,
                p.Content.Title,
                new AuthorDto(p.Author.AliasId, "Anonymous", ""),
                p.Visibility.ToString(),
                p.Moderation.Status.ToString(),
                p.Metrics.ReactionCount,
                p.Metrics.CommentCount,
                p.Metrics.ViewCount,
                p.CreatedAt.Value,
                p.EditedAt,
                p.PublishedAt,
                p.Media.Select(m => m.MediaId.ToString()).ToList(),
                p.Categories.Select(c => c.CategoryTagId.ToString()).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PostDto>(
            request.PageNumber,
            request.PageSize,
            totalCount,
            posts
        );
    }
}
