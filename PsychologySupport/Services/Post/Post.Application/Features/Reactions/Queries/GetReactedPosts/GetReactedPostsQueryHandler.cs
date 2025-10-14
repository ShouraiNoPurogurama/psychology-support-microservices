using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Reactions.Queries.GetReactedPosts;

internal sealed class GetReactedPostsQueryHandler : IQueryHandler<GetReactedPostsQuery, GetReactedPostsResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _actorAccessor;
    private readonly IQueryDbContext _queryContext;

    public GetReactedPostsQueryHandler(
        IPostDbContext context,
        ICurrentActorAccessor actorAccessor,
        IQueryDbContext queryContext)
    {
        _context = context;
        _actorAccessor = actorAccessor;
        _queryContext = queryContext;
    }

    public async Task<GetReactedPostsResult> Handle(GetReactedPostsQuery request, CancellationToken cancellationToken)
    {
        var currentAliasId = _actorAccessor.GetRequiredAliasId();

        // Get reactions by the specified user for posts
        var reactionsQuery = _context.Reactions
            .AsNoTracking()
            .Where(r => r.Author.AliasId == request.AliasId &&
                        !r.IsDeleted &&
                        r.Target.TargetType == ReactionTargetType.Post);

        // Filter by reaction code if specified
        if (!string.IsNullOrEmpty(request.ReactionCode))
        {
            reactionsQuery = reactionsQuery.Where(r => r.Type.Code == request.ReactionCode);
        }

        // Order by reaction date
        reactionsQuery = request.SortDescending
            ? reactionsQuery.OrderByDescending(r => r.ReactedAt)
            : reactionsQuery.OrderBy(r => r.ReactedAt);

        // Get the post IDs from reactions
        var postIdsQuery = reactionsQuery.Select(r => r.Target.TargetId);

        // Get total count
        var totalCount = await postIdsQuery.CountAsync(cancellationToken);

        // Paginate post IDs
        var paginatedPostIds = await postIdsQuery
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Fetch the full posts data
        var postsData = await _context.Posts
            .Include(p => p.Emotions)
            .Include(p => p.Media)
            .Include(p => p.Categories)
                .ThenInclude(pc => pc.CategoryTag)
            .Where(p => paginatedPostIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.Author.AliasId == currentAliasId &&
                    !r.IsDeleted &&
                    r.Target.TargetType == ReactionTargetType.Post &&
                    r.Target.TargetId == p.Id)
            })
            .ToListAsync(cancellationToken);

        // Maintain the order from the reactions query
        var orderedPostsData = paginatedPostIds
            .Select(postId => postsData.FirstOrDefault(pd => pd.Post.Id == postId))
            .Where(pd => pd != null)
            .ToList();

        // Get author information
        var authorIds = orderedPostsData.Select(p => p!.Post.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var postDtos = orderedPostsData.Select(p =>
        {
            var author = authorAliases
                .Select(a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl))
                .FirstOrDefault(a => a.AliasId == p!.Post.Author.AliasId);

            return new PostSummaryDto(
                p!.Post.Id,
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

        return new GetReactedPostsResult(paginatedResult);
    }
}
