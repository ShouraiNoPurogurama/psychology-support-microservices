using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Commands.SoftDeleteComment;

internal sealed class SoftDeleteCommentCommandHandler : ICommandHandler<SoftDeleteCommentCommand, SoftDeleteCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;

    public SoftDeleteCommentCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver)
    {
        _context = context;
        _actorResolver = actorResolver;
    }

    public async Task<SoftDeleteCommentResult> Handle(SoftDeleteCommentCommand request, CancellationToken cancellationToken)
    {
        // Load the comment aggregate
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Comment not found or has been deleted.", "COMMENT_NOT_FOUND");

        // Authorization: Must be author
        if (comment.Author.AliasId != _actorResolver.AliasId)
        {
            throw new ForbiddenException("Only the comment author can delete this comment.", "UNAUTHORIZED_COMMENT_DELETE");
        }

        // Soft delete via domain methods (preferred over hard delete)
        comment.SoftDelete(_actorResolver.AliasId);

        await _context.SaveChangesAsync(cancellationToken);

        return new SoftDeleteCommentResult(
            comment.Id,
            comment.DeletedAt!.Value
        );
    }
}
