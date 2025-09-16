using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Comments.Commands.EditComment;

internal sealed class EditCommentCommandHandler : ICommandHandler<EditCommentCommand, EditCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly IActorResolver _actorResolver;

    public EditCommentCommandHandler(
        IPostDbContext context,
        IActorResolver actorResolver)
    {
        _context = context;
        _actorResolver = actorResolver;
    }

    public async Task<EditCommentResult> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        // Load the comment aggregate
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Comment not found or has been deleted.", "COMMENT_NOT_FOUND");

        // Authorization: Must be author
        if (comment.Author.AliasId != _actorResolver.AliasId)
        {
            throw new ForbiddenException("Only the comment author can edit this comment.", "UNAUTHORIZED_COMMENT_EDIT");
        }

        // Update comment content via domain methods
        try
        {
            comment.UpdateContent(request.Content, _actorResolver.AliasId);
        }
        catch (Domain.Exceptions.InvalidPostDataException ex)
        {
            throw new BadRequestException(ex.Message, "INVALID_COMMENT_CONTENT");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new EditCommentResult(
            comment.Id,
            comment.Content.Value,
            comment.EditedAt
        );
    }
}
