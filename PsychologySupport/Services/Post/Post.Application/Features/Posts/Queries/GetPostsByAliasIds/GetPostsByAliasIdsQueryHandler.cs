using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByAliasIds;

internal sealed class GetPostsByAliasIdsQueryHandler : IQueryHandler<GetPostsByAliasIdsQuery, GetPostsByAliasIdsResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetPostsByAliasIdsQueryHandler(IPostDbContext context, ICurrentActorAccessor actorAccessor)
    {
        _context = context;
        _actorAccessor = actorAccessor;
    }

    public async Task<GetPostsByAliasIdsResult> Handle(GetPostsByAliasIdsQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();

        var baseQuery = _context.Posts
            .Include(p => p.Media.Where(m => !m.IsDeleted))
            .Include(p => p.Categories.Where(c => !c.IsDeleted))
            .Include(p => p.Emotions.Where(e => !e.IsDeleted))
            .AsNoTracking()
            .Where(p => request.AliasIds.Contains(p.Author.AliasId) &&
                        !p.IsDeleted &&
                        p.Visibility == PostVisibility.Public)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.IsOnPost &&
                    !r.IsDeleted &&
                    r.Target.TargetId == p.Id &&
                    r.Author.AliasId == aliasId)
            })
            .OrderByDescending(p => p.Post.PublishedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var postsData = await baseQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(p => new PostSummaryDto(
                p.Post.Id,
                p.Post.Content.Title,
                p.Post.Content.Value,
                p.IsReacted,
                new AuthorDto(aliasId, "", ""),
                p.Post.Visibility,
                p.Post.PublishedAt,
                p.Post.EditedAt,
                p.Post.Metrics.ReactionCount,
                p.Post.Metrics.CommentCount,
                p.Post.Metrics.ViewCount,
                p.Post.HasMedia,
                p.Post.Media.Where(m => !m.IsDeleted).Select(m => new MediaItemDto(m.Id, m.MediaUrl)).ToList(),
                p.Post.Categories.Where(c => !c.IsDeleted).Select(c => c.CategoryTagId).ToList(),
                p.Post.Emotions.Where(e => !e.IsDeleted).Select(e => e.EmotionTagId).ToList()
            ))
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postsData
        );

        return new GetPostsByAliasIdsResult(paginatedResult);
    }
}
