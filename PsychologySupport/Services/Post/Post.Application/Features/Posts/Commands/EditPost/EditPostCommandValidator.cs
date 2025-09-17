using FluentValidation;

namespace Post.Application.Features.Posts.Commands.EditPost;

public class EditPostCommandValidator : AbstractValidator<EditPostCommand>
{
    public EditPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết cần chỉnh sửa.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Vui lòng nhập nội dung bài viết.")
            .MaximumLength(10000)
            .WithMessage("Nội dung bài viết không được vượt quá 10.000 ký tự.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Tiêu đề không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.MediaUrls)
            .Must(urls => urls == null || urls.Count <= 10)
            .WithMessage("Bạn chỉ có thể đính kèm tối đa 10 Media cho mỗi bài viết.")
            .When(x => x.MediaUrls != null);

        RuleForEach(x => x.MediaUrls)
            .NotEmpty()
            .WithMessage("Vui lòng nhập đường dẫn Media.")
            .Must(BeValidUrl)
            .WithMessage("Định dạng đường dẫn Media không hợp lệ.")
            .When(x => x.MediaUrls != null);

        RuleFor(x => x.CategoryTagIds)
            .Must(tags => tags == null || tags.Count <= 5)
            .WithMessage("Bạn chỉ có thể chọn tối đa 5 Tag cho mỗi bài viết.")
            .When(x => x.CategoryTagIds != null);
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
