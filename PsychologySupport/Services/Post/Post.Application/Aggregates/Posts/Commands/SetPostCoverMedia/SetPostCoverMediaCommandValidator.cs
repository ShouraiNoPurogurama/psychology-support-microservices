using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.SetPostCoverMedia;

public sealed class SetPostCoverMediaCommandValidator : AbstractValidator<SetPostCoverMediaCommand>
{
    public SetPostCoverMediaCommandValidator()
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
    }
}
