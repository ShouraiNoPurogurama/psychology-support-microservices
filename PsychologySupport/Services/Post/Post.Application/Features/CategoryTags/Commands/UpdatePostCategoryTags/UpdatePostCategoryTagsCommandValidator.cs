using FluentValidation;

namespace Post.Application.Features.CategoryTags.Commands.UpdatePostCategoryTags;

public sealed class UpdatePostCategoryTagsCommandValidator : AbstractValidator<UpdatePostCategoryTagsCommand>
{
    public UpdatePostCategoryTagsCommandValidator()
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
            .Must(ids => ids.Count() <= 5)
            .WithMessage("Bạn chỉ có thể chọn tối đa 5 Tag cho mỗi bài viết.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Vui lòng chọn các Tag hợp lệ.")
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithMessage("Không được chọn trùng lặp Tag.");
    }
}
