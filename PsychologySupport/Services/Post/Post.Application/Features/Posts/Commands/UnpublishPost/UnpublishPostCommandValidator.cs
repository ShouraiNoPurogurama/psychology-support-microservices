using FluentValidation;

namespace Post.Application.Features.Posts.Commands.UnpublishPost;

public sealed class UnpublishPostCommandValidator : AbstractValidator<UnpublishPostCommand>
{
    public UnpublishPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");
    }
}
