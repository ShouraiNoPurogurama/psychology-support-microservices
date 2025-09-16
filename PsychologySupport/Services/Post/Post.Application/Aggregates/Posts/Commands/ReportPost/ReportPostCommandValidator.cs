using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.ReportPost;

public sealed class ReportPostCommandValidator : AbstractValidator<ReportPostCommand>
{
    public ReportPostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết cần báo cáo.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Vui lòng nhập lý do báo cáo.")
            .MaximumLength(500)
            .WithMessage("Lý do báo cáo không được vượt quá 500 ký tự.")
            .MinimumLength(10)
            .WithMessage("Lý do báo cáo phải có ít nhất 10 ký tự.");
    }
}
