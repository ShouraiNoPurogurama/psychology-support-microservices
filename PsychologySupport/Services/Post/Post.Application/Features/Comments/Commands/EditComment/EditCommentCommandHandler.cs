using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;

namespace Post.Application.Features.Comments.Commands.EditComment;

internal sealed class EditCommentCommandHandler : ICommandHandler<EditCommentCommand, EditCommentResult>
{
    private readonly IPostDbContext _context;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public EditCommentCommandHandler(
        IPostDbContext context,
        ICurrentActorAccessor currentActorAccessor)
    {
        _context = context;
        _currentActorAccessor = currentActorAccessor;
    }

    public async Task<EditCommentResult> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        // Load the comment aggregate
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new NotFoundException("Không tìm thấy bình luận.", "COMMENT_NOT_FOUND");

        // Authorization: Must be author
        if (comment.Author.AliasId != _currentActorAccessor.GetRequiredAliasId())
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa bình luận này.", "UNAUTHORIZED_COMMENT_EDIT");
        }

        // Update comment content via domain methods
        try
        {
            comment.UpdateContent(request.Content, _currentActorAccessor.GetRequiredAliasId());
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
