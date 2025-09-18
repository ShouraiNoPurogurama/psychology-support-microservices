using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Features.Reactions.Queries.GetReactionSummaryForPost;

internal sealed class GetReactionSummaryForPostQueryHandler : IQueryHandler<GetReactionSummaryForPostQuery, GetReactionSummaryForPostResult>
{
    private readonly IPostDbContext _postDb;

    public GetReactionSummaryForPostQueryHandler(IPostDbContext postDb)
    {
        _postDb = postDb;
    }

    public async Task<GetReactionSummaryForPostResult> Handle(GetReactionSummaryForPostQuery request, CancellationToken cancellationToken)
    {
        // Ensure post exists
        var exists = await _postDb.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PostId && !p.IsDeleted, cancellationToken);
        if (!exists)
            throw new NotFoundException("Post not found or deleted", "POST_NOT_FOUND");

        // Query reactions for this post
        var reactions = await _postDb.Reactions
            .AsNoTracking()
            .Where(r => r.Target.TargetId == request.PostId &&
                        r.Target.TargetType == ReactionTargetType.Post &&
                        !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var reactionCounts = reactions
            .GroupBy(r => r.Type.Code)
            .ToDictionary(g => g.Key, g => g.Count());
        var total = reactions.Count;

        return new GetReactionSummaryForPostResult(request.PostId, total, reactionCounts);
    }
}

