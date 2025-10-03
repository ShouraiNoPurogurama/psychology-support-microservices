using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Application.Features.Posts.Queries.GetPosts;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByIds;

internal sealed class GetPostsByIdsHandler : IQueryHandler<GetPostsQuery, PaginatedResult<PostSummaryDto>>
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

    public async Task<PaginatedResult<PostSummaryDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();


        var query = _context.Posts
            .Where(p => !p.IsDeleted)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.IsOnPost
                    && r.Target.TargetId == p.Id
                    && r.Author.AliasId == aliasId
                )
            })
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Visibility) &&
            Enum.TryParse<PostVisibility>(request.Visibility, true, out var visibility))
        {
            query = query.Where(p => p.Post.Visibility == visibility);
        }

        if (request.CategoryTagIds?.Any() == true)
        {
            query = query.Where(p => p.Post.Categories.Any(pc => request.CategoryTagIds.Contains(pc.CategoryTagId)));
        }

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
                    p.Post.HasMedia);
            }
        );

        return new PaginatedResult<PostSummaryDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            postDtos
        );
    }
}