using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.EditPost;

public class EditPostCommandValidator : AbstractValidator<EditPostCommand>
{
    public EditPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(10000)
            .WithMessage("Content cannot exceed 10,000 characters");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.MediaUrls)
            .Must(urls => urls == null || urls.Count <= 10)
            .WithMessage("Cannot have more than 10 media attachments")
            .When(x => x.MediaUrls != null);

        RuleForEach(x => x.MediaUrls)
            .NotEmpty()
            .WithMessage("Media URL cannot be empty")
            .Must(BeValidUrl)
            .WithMessage("Invalid media URL format")
            .When(x => x.MediaUrls != null);

        RuleFor(x => x.CategoryTagIds)
            .Must(tags => tags == null || tags.Count <= 5)
            .WithMessage("Cannot have more than 5 category tags")
            .When(x => x.CategoryTagIds != null);
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
