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
        
        // Query posts by alias IDs with pagination and AsNoTracking
        var baseQuery = _context.Posts
                .Include(p => p.Media)
                .Include(p => p.Categories)
                .Include(p => p.Emotions)
                .AsNoTracking()
                .Where(p => request.AliasIds.Contains(p.Author.AliasId) &&
                            !p.IsDeleted &&
                            p.Visibility == PostVisibility.Public)
                .Select(p => new
                {
                    Post = p,
                    IsReacted = _context.Reactions.Any(r =>
                        r.IsOnPost
                        && !r.IsDeleted
                        && r.Target.TargetId == p.Id
                        && r.Author.AliasId == aliasId)
                })
                .OrderByDescending(p => p.Post.PublishedAt)
            ;

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.Select(p => new PostSummaryDto(
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
            p.Post.Media.Select(m => new MediaItemDto(m.Id, m.MediaUrl)).ToList(),
            p.Post.Categories.Select(c => c.Id).ToList(),
            p.Post.Emotions.Select(e => e.Id).ToList()
        ));
        
        var postsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
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