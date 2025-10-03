using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;

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
            throw new NotFoundException("Category tag not found.", "CATEGORY_TAG_NOT_FOUND");

        // Get posts by category tag with pagination and AsNoTracking
        var query = _context.PostCategories
            .AsNoTracking()
            .Where(pc => pc.CategoryTagId == request.CategoryTagId && !pc.IsDeleted)
            .Join(_context.Posts,
                pc => pc.PostId,
                p => p.Id,
                (pc, p) => p)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r => r.IsOnPost
                && r.Target.TargetId == p.Id
                && r.Author.AliasId == aliasId)
            });

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Get posts data with pagination
        var postsData = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        // Get author display names from query context (no EF join)
        var authorIds = postsData.Select(p => p.Post.Author.AliasId).Distinct().ToList();
        var authorAliases = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .Where(a => authorIds.Contains(a.AliasId))
            .ToListAsync(cancellationToken);

        // Merge in memory (following manifesto rules)
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
                p.Post.HasMedia);
        }).ToList();

        var paginatedResult = new PaginatedResult<PostSummaryDto>(
            request.Page,
            request.Size,
            totalCount,
            postDtos
        );

        return new GetPostsByTagResult(paginatedResult);
    }
}
