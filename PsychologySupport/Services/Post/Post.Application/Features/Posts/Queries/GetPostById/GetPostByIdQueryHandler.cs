using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Features.CategoryTags.Dtos;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostById;

internal sealed class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, GetPostByIdResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;
    private readonly ICurrentActorAccessor _actorAccessor;

    public GetPostByIdQueryHandler(IPostDbContext context, ICurrentActorAccessor actorAccessor, IQueryDbContext queryContext)
    {
        _context = context;
        _actorAccessor = actorAccessor;
        _queryContext = queryContext;
    }

    public async Task<GetPostByIdResult> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var aliasId = _actorAccessor.GetRequiredAliasId();

        var query = _context.Posts
            .Include(p => p.Emotions.Where(e => !e.IsDeleted))
            .Include(p => p.Media.Where(m => !m.IsDeleted))
            .Include(p => p.Categories.Where(pc => !pc.IsDeleted))
                .ThenInclude(pc => pc.CategoryTag)
            .Where(p => p.Id == request.PostId && !p.IsDeleted);

        var postData = await query
            .Select(p => new
            {
                Post = p,
                IsReacted = _context.Reactions.Any(r =>
                    r.Author.AliasId == aliasId &&
                    !r.IsDeleted &&
                    r.Target.TargetType == ReactionTargetType.Post &&
                    r.Target.TargetId == p.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (postData is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        var author = await _queryContext.AliasVersionReplica
            .Where(ap => ap.AliasId == postData.Post.Author.AliasId)
            .Select(ap => new AuthorDto(ap.AliasId, ap.Label, ap.AvatarUrl))
            .FirstOrDefaultAsync(cancellationToken);

        var postDto = new PostSummaryDto(
            postData.Post.Id,
            postData.Post.Content.Title,
            postData.Post.Content.Value,
            postData.IsReacted,
            author ?? new AuthorDto(postData.Post.Author.AliasId, "Anonymous", null),
            postData.Post.Visibility,
            postData.Post.PublishedAt,
            postData.Post.EditedAt,
            postData.Post.Metrics.ReactionCount,
            postData.Post.Metrics.CommentCount,
            postData.Post.Metrics.ViewCount,
            postData.Post.HasMedia,
            postData.Post.Media
                .Where(m => !m.IsDeleted)
                .Select(m => new MediaItemDto(m.Id, m.MediaUrl))
                .ToList(),
            postData.Post.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => c.CategoryTagId)
                .ToList(),
            postData.Post.Emotions
                .Where(e => !e.IsDeleted)
                .Select(e => e.EmotionTagId)
                .ToList()
        );

        return new GetPostByIdResult(postDto);
    }
}
