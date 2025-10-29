using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Gifts.Enums;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByTag;

internal sealed class GetPostsByTagQueryHandler : IQueryHandler<GetPostsByTagQuery, GetPostsByTagResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetPostsByTagQueryHandler(IPostDbContext context, IQueryDbContext queryContext, ICurrentActorAccessor actorAccessor)
    {
        _context = context;
        _queryContext = queryContext;
        _actorAccessor = actorAccessor;
    }

    public async Task<GetPostsByTagResult> Handle(GetPostsByTagQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();

        // Verify category tag exists
        var categoryExists = await _context.CategoryTags
            .AsNoTracking()
            .AnyAsync(ct => ct.Id == request.CategoryTagId, cancellationToken);

        if (!categoryExists)
            throw new NotFoundException("Không tìm thấy danh mục.", "CATEGORY_TAG_NOT_FOUND");

        var baseQuery = _context.Posts
            .Include(p => p.Categories)
            .Include(p => p.Emotions)
            .Include(p => p.Media)
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Categories.Any(c => c.CategoryTagId == request.CategoryTagId && !c.IsDeleted))
            .OrderByDescending(p => p.CreatedAt);

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var postsData = await baseQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(p => new
            {
                Post = new
                {
                    p.Id,
                    p.Content.Title,
                    p.Content.Value,
                    p.Visibility,
                    p.PublishedAt,
                    p.EditedAt,
                    p.Metrics.ReactionCount,
                    p.Metrics.CommentCount,
                    p.Metrics.ViewCount,
                    p.HasMedia,
                    AuthorAliasId = p.Author.AliasId,
                    Media = p.Media.Select(m => new MediaItemDto(m.MediaId, m.MediaUrl)).ToList(),
                    CategoryIds = p.Categories.Select(c => c.CategoryTagId).ToList(),
                    EmotionIds = p.Emotions.Select(e => e.EmotionTagId).ToList()
                },
                IsReacted = _context.Reactions.Any(r =>
                    r.Author.AliasId == aliasId
                    && !r.IsDeleted
                    && r.Target.TargetId == p.Id
                    && r.Target.TargetType == ReactionTargetType.Post
                )
            })
            .ToListAsync(cancellationToken);

        // Get author display names from query context (no EF join)
        var authorIds = postsData.Select(p => p.Post.AuthorAliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToDictionaryAsync(a => a.AliasId, a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl), cancellationToken);

        var giftAttachesQuery = _context.GiftAttaches
            .AsNoTracking()
            .Where(g => g.Target.TargetType == nameof(GiftTargetType.Post) &&
                        postsData.Select(p => p.Post.Id).Contains(g.Target.TargetId) &&
                        !g.IsDeleted);
        
        var giftAttachCountMap = await giftAttachesQuery
            .GroupBy(g => g.Target.TargetId)
            .Select(g => new
            {
                PostId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(g => g.PostId, g => g.Count, cancellationToken);
        
        // Merge in memory (following manifesto rules)
        var postDtos = postsData.Select(p =>
            {
                var post = p.Post;
                var author = authorAliases.GetValueOrDefault(post.AuthorAliasId)
                             ?? new AuthorDto(post.AuthorAliasId, "Anonymous", null);

                return new PostSummaryDto(
                    post.Id,
                    post.Title,
                    post.Value,
                    p.IsReacted,
                    author,
                    post.Visibility,
                    post.PublishedAt,
                    post.EditedAt,
                    post.ReactionCount,
                    post.CommentCount,
                    post.ViewCount,
                    giftAttachCountMap.TryGetValue(post.Id, out var value) ? value : 0,
                    post.HasMedia,
                    post.Media,
                    post.CategoryIds,
                    post.EmotionIds
                );
            })
            .ToList();

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postDtos
        );

        return new GetPostsByTagResult(paginatedResult);
    }
}