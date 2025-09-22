using FluentValidation;

namespace Post.Application.Features.Posts.Commands.ToggleCommentsLock;

public sealed class ToggleCommentsLockCommandValidator : AbstractValidator<ToggleCommentsLockCommand>
{
    public ToggleCommentsLockCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");
    }
}
