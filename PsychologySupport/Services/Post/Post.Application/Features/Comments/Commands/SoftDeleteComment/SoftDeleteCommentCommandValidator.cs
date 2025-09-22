using FluentValidation;

namespace Post.Application.Features.Comments.Commands.SoftDeleteComment;

public sealed class SoftDeleteCommentCommandValidator : AbstractValidator<SoftDeleteCommentCommand>
{
    public SoftDeleteCommentCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bình luận cần xóa.");
    }
}
