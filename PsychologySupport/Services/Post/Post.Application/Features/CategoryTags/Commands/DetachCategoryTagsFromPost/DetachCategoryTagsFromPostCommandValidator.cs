using FluentValidation;

namespace Post.Application.Features.CategoryTags.Commands.DetachCategoryTagsFromPost;

public sealed class DetachCategoryTagsFromPostCommandValidator : AbstractValidator<DetachCategoryTagsFromPostCommand>
{
    public DetachCategoryTagsFromPostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp mã định danh idempotency.");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết hợp lệ.");

        RuleFor(x => x.CategoryTagIds)
            .NotNull()
            .WithMessage("Danh sách Tag không được để trống.")
            .Must(ids => ids.Any())
            .WithMessage("Vui lòng chọn ít nhất một Tag.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Vui lòng chọn các Tag hợp lệ.")
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Không được chọn trùng lặp Tag.");
    }
}
