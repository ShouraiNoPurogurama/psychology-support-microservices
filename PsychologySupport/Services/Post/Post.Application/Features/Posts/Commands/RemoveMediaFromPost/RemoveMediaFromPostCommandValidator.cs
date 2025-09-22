using FluentValidation;

namespace Post.Application.Features.Posts.Commands.RemoveMediaFromPost;

public sealed class RemoveMediaFromPostCommandValidator : AbstractValidator<RemoveMediaFromPostCommand>
{
    public RemoveMediaFromPostCommandValidator()
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
