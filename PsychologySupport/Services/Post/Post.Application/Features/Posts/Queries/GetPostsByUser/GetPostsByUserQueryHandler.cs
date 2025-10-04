using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostsByUser;

internal sealed class GetPostsByUserQueryHandler : IQueryHandler<GetPostsByUserQuery, GetPostsByUserResult>
{
    private readonly IPostDbContext _context;

    public GetPostsByUserQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetPostsByUserResult> Handle(GetPostsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .Include(p => p.Media)
            .Include(p => p.Categories)
            .ThenInclude(pc => pc.CategoryTag)
            .Include(p => p.Emotions)
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
                p.Visibility,
                p.Moderation.Status,
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

        var paginatedResult = new PaginatedResult<PostDto>(
            request.PageNumber,
            request.PageSize,
            totalCount,
            posts
        );

        return new GetPostsByUserResult(paginatedResult);
    }
}
