using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByIds;

internal sealed class GetPostsByIdsHandler : IQueryHandler<GetPostsByIdsQuery, GetPostsByIdsResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetPostsByIdsHandler(IPostDbContext context, ICurrentActorAccessor actorAccessor, IQueryDbContext queryContext)
    {
        _context = context;
        _actorAccessor = actorAccessor;
        _queryContext = queryContext;
    }

    public async Task<GetPostsByIdsResult> Handle(GetPostsByIdsQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();


        var query = _context.Posts
            .Include(p => p.Media)
            .Include(p => p.Categories)
            .Include(p => p.Emotions)
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.Author.AliasId == aliasId
                    && !r.IsDeleted
                    && r.Target.TargetId == p.Id
                    && r.IsOnPost
                )
            })
            .AsQueryable();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "createdat" => request.SortDescending
                ? query.OrderByDescending(p => p.Post.CreatedAt)
                : query.OrderBy(p => p.Post.CreatedAt),
            "reactioncount" => request.SortDescending
                ? query.OrderByDescending(p => p.Post.Metrics.ReactionCount)
                : query.OrderBy(p => p.Post.Metrics.ReactionCount),
            "commentcount" => request.SortDescending
                ? query.OrderByDescending(p => p.Post.Metrics.CommentCount)
                : query.OrderBy(p => p.Post.Metrics.CommentCount),
            _ => query.OrderByDescending(p => p.Post.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Post.Media)
            .Include(p => p.Post.Categories)
            .ThenInclude(pc => pc.CategoryTag)
            .ToListAsync(cancellationToken: cancellationToken);

        // Get author display names from query context (no EF join)
        var authorIds = posts.Select(p => p.Post.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        // Merge in memory (following manifesto rules)
        var postDtos = posts.Select(p =>
            {
                var author = authorAliases
                    .Select(a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl))
                    .FirstOrDefault(a => a.AliasId == p.Post.Author.AliasId);

                return new PostSummaryDto(
                    p.Post.Id,
                    p.Post.Content.Title,
                    p.Post.Content.Value,
                    p.IsReacted,
                    author ?? new AuthorDto(p.Post.Author.AliasId, "Anonymous", null),
                    p.Post.Visibility,
                    p.Post.PublishedAt,
                    p.Post.EditedAt,
                    p.Post.Metrics.ReactionCount,
                    p.Post.Metrics.CommentCount,
                    p.Post.Metrics.ViewCount,
                    p.Post.HasMedia,
                    p.Post.Media.Select(m => new MediaItemDto(m.Id, m.MediaUrl)).ToList(),
                    p.Post.Categories.Select(c => c.Id).ToList(),
                    p.Post.Emotions.Select(e => e.Id).ToList()
                    );
            }
        );

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            postDtos
        );

        return new GetPostsByIdsResult(paginatedResult);
    }
}
