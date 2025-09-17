using FluentValidation;

namespace Post.Application.Features.Posts.Commands.AttachMediaToPost;

public sealed class AttachMediaToPostCommandValidator : AbstractValidator<AttachMediaToPostCommand>
{
    public AttachMediaToPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("MediaId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Position.HasValue)
            .WithMessage("Position must be non-negative when specified.");
    }
}
