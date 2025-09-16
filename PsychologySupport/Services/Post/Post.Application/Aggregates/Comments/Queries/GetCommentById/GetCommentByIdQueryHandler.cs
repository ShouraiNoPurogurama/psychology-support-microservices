using BuildingBlocks.CQRS;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentById;

internal sealed class GetCommentByIdQueryHandler : IQueryHandler<GetCommentByIdQuery, GetCommentByIdResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;

    public GetCommentByIdQueryHandler(IPostDbContext context, IQueryDbContext queryContext)
    {
        _context = context;
        _queryContext = queryContext;
    }

    public async Task<GetCommentByIdResult> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        // Get comment with AsNoTracking
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
            return new GetCommentByIdResult(null);

        // Get author display name from query context (no EF join)
        var authorAlias = await _queryContext.AliasVersionReplica
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AliasId == comment.Author.AliasId, cancellationToken);

        var commentDto = new CommentDto(
            comment.Id,
            comment.PostId,
            comment.Author.AliasId,
            authorAlias.Label,
            comment.Content.Value,
            comment.ReplyCount,
            comment.CreatedAt,
            comment.EditedAt,
            comment.IsDeleted
        );

        return new GetCommentByIdResult(commentDto);
    }
}
