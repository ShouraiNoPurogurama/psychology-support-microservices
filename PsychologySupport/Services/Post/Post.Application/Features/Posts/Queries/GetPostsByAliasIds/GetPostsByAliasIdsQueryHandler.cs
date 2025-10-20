using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostsByAliasIds;

internal sealed class GetPostsByAliasIdsQueryHandler : IQueryHandler<GetPostsByAliasIdsQuery, GetPostsByAliasIdsResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetPostsByAliasIdsQueryHandler(IPostDbContext context, ICurrentActorAccessor actorAccessor, IQueryDbContext queryContext)
    {
        _context = context;
        _actorAccessor = actorAccessor;
        _queryContext = queryContext;
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
            .ToListAsync(cancellationToken);

        var authorIds = postsData.Select(p => p.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        var postDtos = postsData.Select(p =>
        {
            var author = authorAliases
                .Select(a => new AuthorDto(a.AliasId, a.Label, a.AvatarUrl))
                .FirstOrDefault(a => a.AliasId == p.Author.AliasId);

            return new PostSummaryDto(
                p.Id,
                p.Content.Title,
                p.Content.Value,
                _context.Reactions.Any(r =>
                    r.Target.TargetType == ReactionTargetType.Post
                    && !r.IsDeleted
                    && r.Target.TargetId == p.Id
                    && r.Author.AliasId == aliasId),
                author ?? new AuthorDto(p.Author.AliasId, "Anonymous", null),
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
            );
        }).ToList();


        // Create complete PaginatedResult object
        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postDtos
        );

        return new GetPostsByAliasIdsResult(paginatedResult);
    }
}