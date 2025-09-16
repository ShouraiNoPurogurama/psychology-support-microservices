using FluentValidation;

namespace Post.Application.Aggregates.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết để bình luận.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Vui lòng nhập nội dung bình luận.")
            .MaximumLength(2000)
            .WithMessage("Nội dung bình luận không được vượt quá 2.000 ký tự.");

        RuleFor(x => x.ParentCommentId)
            .NotEqual(Guid.Empty)
            .WithMessage("Vui lòng chọn bình luận cha hợp lệ.")
            .When(x => x.ParentCommentId.HasValue);
    }
}
