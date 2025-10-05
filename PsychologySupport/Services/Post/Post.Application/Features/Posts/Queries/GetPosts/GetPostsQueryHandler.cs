using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Posts.Queries.GetPosts;

internal sealed class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, GetPostsResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _actorAccessor;
    private readonly IQueryDbContext _queryContext;

    public GetPostsQueryHandler(IPostDbContext context, ICurrentActorAccessor actorAccessor, IQueryDbContext queryContext)
    {
        _context = context;
        _actorAccessor = actorAccessor;
        _queryContext = queryContext;
    }

    public async Task<GetPostsResult> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();

        var query = _context.Posts
            .Include(p => p.Emotions)
            .Include(p => p.Media)
            .Include(p => p.Categories)
                .ThenInclude(pc => pc.CategoryTag)
            .Where(p => !p.IsDeleted);

        if (request.Ids.Any())
        {
            query = query.Where(p => request.Ids.Contains(p.Id));
        }

        // Apply filters
        if (!string.IsNullOrEmpty(request.Visibility) &&
            Enum.TryParse<PostVisibility>(request.Visibility, true, out var visibility))
        {
            query = query.Where(p => p.Visibility == visibility);
        }

        if (request.CategoryTagIds?.Any() == true)
        {
            query = query.Where(p =>
                p.Categories.Any(pc => !pc.IsDeleted && request.CategoryTagIds.Contains(pc.CategoryTagId)));
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

        var postsData = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.Author.AliasId == aliasId &&
                    !r.IsDeleted &&
                    r.Target.TargetType == ReactionTargetType.Post &&
                    r.Target.TargetId == p.Id)
            })
            .ToListAsync(cancellationToken);

        var authorIds = postsData.Select(p => p.Post.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        var postDtos = postsData.Select(p =>
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
                p.Post.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => c.CategoryTagId)
                    .ToList(),
                p.Post.Emotions
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.EmotionTagId)
                    .ToList()
            );
        }).ToList();

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            postDtos
        );

        return new GetPostsResult(paginatedResult);
    }
}
