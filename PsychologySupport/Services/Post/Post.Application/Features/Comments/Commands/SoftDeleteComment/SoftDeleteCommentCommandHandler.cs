using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Comments;

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
        var result = await (
                from c in _context.Comments
                where c.Id == request.CommentId && !c.IsDeleted
                join pc in _context.Comments
                    on c.Hierarchy.ParentCommentId equals pc.Id into
                    pcg //pcg chỉ chứa duy nhất các comment cha (pc) khớp với comment con (c) hiện tại.
                join post in _context.Posts
                    on c.PostId equals post.Id
                where !post.IsDeleted
                from parent in pcg.DefaultIfEmpty()
                select new { Comment = c, ParentComment = parent, Post = post }
            )
            .FirstOrDefaultAsync(cancellationToken);


        if (result?.Comment == null)
            throw new NotFoundException("Comment not found or has been deleted.", "COMMENT_NOT_FOUND");

        var comment = result.Comment;

        // Authorization: Must be author
        if (comment.Author.AliasId != _currentActorAccessor.GetRequiredAliasId())
        {
            throw new ForbiddenException("Only the comment author can delete this comment.", "UNAUTHORIZED_COMMENT_DELETE");
        }

        Comment? parentComment = null;

        if (comment.Hierarchy.IsReply)
        {
            parentComment = result.ParentComment;
        }

        // Soft delete via domain methods (preferred over hard delete)
        comment.SoftDelete(result.Post, parentComment, _currentActorAccessor.GetRequiredAliasId());

        // Decrement comment count and emit domain event for alias counters
        var post = await _context.Posts.FirstAsync(p => p.Id == comment.PostId, cancellationToken);
        post.DecrementCommentCount();
        post.RemoveComment(comment.Author.AliasId);

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