using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Aggregates.Reactions.Enums;

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

        // Query posts by alias IDs with pagination and AsNoTracking
        var baseQuery = _context.Posts
                .Include(p => p.Media)
                .Include(p => p.Categories)
                .Include(p => p.Emotions)
                .AsNoTracking()
                .Where(p => request.AliasIds.Contains(p.Author.AliasId) &&
                            !p.IsDeleted &&
                            p.Visibility == PostVisibility.Public)
                .OrderByDescending(p => p.PublishedAt)
            ;

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var postsData = await baseQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(p => new PostSummaryDto(
                p.Id,
                p.Content.Title,
                p.Content.Value,
                _context.Reactions.Any(r =>
                    r.Target.TargetType == ReactionTargetType.Post
                    && !r.IsDeleted
                    && r.Target.TargetId == p.Id
                    && r.Author.AliasId == aliasId),
                new AuthorDto(aliasId, "", ""),
                p.Visibility,
                p.PublishedAt,
                p.EditedAt,
                p.Metrics.ReactionCount,
                p.Metrics.CommentCount,
                p.Metrics.ViewCount,
                p.HasMedia,
                p.Media.Select(m => new MediaItemDto(m.Id, m.MediaUrl)).ToList(),
                p.Categories.Select(c => c.CategoryTagId).ToList(),
                p.Emotions.Select(e => e.EmotionTagId).ToList()
            ))
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