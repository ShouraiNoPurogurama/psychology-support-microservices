using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Reactions.Queries.GetUserReactionForPost;

internal sealed class
    GetUserReactionForPostQueryHandler : IQueryHandler<GetUserReactionForPostQuery, GetUserReactionForPostResult>
{
    private readonly IPostDbContext _context;

    public GetUserReactionForPostQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserReactionForPostResult> Handle(GetUserReactionForPostQuery request,
        CancellationToken cancellationToken)
    {
        // Verify post exists
        var postExists = await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);

        if (!postExists)
            throw new NotFoundException("Không tìm thấy bài viết.", "POST_NOT_FOUND");

        // Get user's reaction for this post with AsNoTracking
        var reaction = await _context.Reactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Target.TargetType == ReactionTargetType.Post
                     && r.Target.TargetId == request.PostId
                     && r.Author.AliasId == request.AliasId && !r.IsDeleted,
                cancellationToken);

        return new GetUserReactionForPostResult(
            reaction?.Id,
            request.PostId,
            request.AliasId,
            reaction.Type.Code,
            reaction.CreatedAt,
            reaction != null
        );
    }
}