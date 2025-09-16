using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.RemoveMediaFromPost;

public sealed class RemoveMediaFromPostCommandValidator : AbstractValidator<RemoveMediaFromPostCommand>
{
    public RemoveMediaFromPostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết hợp lệ.");

        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn Media cần gỡ bỏ.");
    }
}
