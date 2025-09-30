using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;

namespace Post.Application.Features.Comments.Commands.SoftDeleteComment;

internal sealed class SoftDeleteCommentCommandHandler : ICommandHandler<SoftDeleteCommentCommand, SoftDeleteCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;

    public SoftDeleteCommentCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor,
        IOutboxWriter outboxWriter)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
        _outboxWriter = outboxWriter;
    }

    public async Task<SoftDeleteCommentResult> Handle(SoftDeleteCommentCommand request, CancellationToken cancellationToken)
    {
        // Load the comment aggregate
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Comment not found or has been deleted.", "COMMENT_NOT_FOUND");

        // Authorization: Must be author
        if (comment.Author.AliasId != _currentActorAccessor.GetRequiredAliasId())
        {
            throw new ForbiddenException("Only the comment author can delete this comment.", "UNAUTHORIZED_COMMENT_DELETE");
        }

        // Soft delete via domain methods (preferred over hard delete)
        comment.SoftDelete(_currentActorAccessor.GetRequiredAliasId());

        // Publish integration event for downstream services
        await _outboxWriter.WriteAsync(
            new CommentDeletedIntegrationEvent(
                comment.Id, 
                comment.PostId, 
                _currentActorAccessor.GetRequiredAliasId(), 
                comment.DeletedAt!.Value),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new SoftDeleteCommentResult(
            comment.Id,
            comment.DeletedAt!.Value
        );
    }
}
