using FluentValidation;

namespace Post.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency key is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(10000)
            .WithMessage("Content cannot exceed 10000 characters");

        RuleFor(x => x.Title)
            .MaximumLength(255)
            .WithMessage("Title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.MediaIds)
            .Must(HaveUniqueMediaIds)
            .WithMessage("Media IDs must be unique")
            .When(x => x.MediaIds != null);

        RuleForEach(x => x.MediaIds)
            .NotEmpty()
            .WithMessage("Media ID cannot be empty")
            .When(x => x.MediaIds != null);
    }

    private static bool HaveUniqueMediaIds(IEnumerable<Guid>? mediaIds)
    {
        if (mediaIds == null) return true;
        var mediaIdsList = mediaIds.ToList();
        return mediaIdsList.Count == mediaIdsList.Distinct().Count();
    }
}
