using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;

public sealed class AttachMediaToPostCommandValidator : AbstractValidator<AttachMediaToPostCommand>
{
    public AttachMediaToPostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết hợp lệ.");

        RuleFor(x => x.MediaIds)
            .NotNull()
            .WithMessage("Danh sách Media không được để trống.")
            .Must(ids => ids.Any())
            .WithMessage("Vui lòng chọn ít nhất một Media.")
            .Must(ids => ids.Count() <= 10)
            .WithMessage("Bạn chỉ có thể đính kèm tối đa 10 Media cho mỗi bài viết.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Vui lòng chọn các Media hợp lệ.")
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Không được chọn trùng lặp Media.");
    }
}
