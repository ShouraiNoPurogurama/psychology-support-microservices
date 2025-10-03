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

        RuleFor(x => x.Medias.Select(m => m.MediaId))
            .Must(HaveUniqueMediaIds)
            .WithMessage("Media IDs must be unique")
            .When(x => x.Medias.Any());

        RuleForEach(x => x.Medias.Select(m => m.MediaId))
            .NotEmpty()
            .WithMessage("Media ID cannot be empty")
            .When(x => x.Medias.Any());
    }

    private static bool HaveUniqueMediaIds(IEnumerable<Guid>? mediaIds)
    {
        if (mediaIds == null) return true;
        var mediaIdsList = mediaIds.ToList();
        return mediaIdsList.Count == mediaIdsList.Distinct().Count();
    }
}
