using FluentValidation;

namespace Post.Application.Aggregates.Comments.Commands.EditComment;

public sealed class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    public EditCommentCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bình luận cần chỉnh sửa.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Vui lòng nhập nội dung bình luận.")
            .MaximumLength(5000)
            .WithMessage("Nội dung bình luận không được vượt quá 5.000 ký tự.")
            .MinimumLength(1)
            .WithMessage("Nội dung bình luận phải có ít nhất 1 ký tự.");
    }
}
