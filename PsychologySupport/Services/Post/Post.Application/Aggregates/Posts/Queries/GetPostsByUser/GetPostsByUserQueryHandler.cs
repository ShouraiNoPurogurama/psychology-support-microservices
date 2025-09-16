using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Data;
using Post.Application.Aggregates.Posts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByUser;

internal sealed class GetPostsByUserQueryHandler : IQueryHandler<GetPostsByUserQuery, PaginatedResult<PostDto>>
{
    private readonly IPostDbContext _context;

    public GetPostsByUserQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<PostDto>> Handle(GetPostsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .Where(p => p.Author.AliasId == request.AuthorAliasId && !p.IsDeleted)
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
                p.CreatedAt,
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
