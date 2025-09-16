using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.RestorePost;

public sealed class RestorePostCommandValidator : AbstractValidator<RestorePostCommand>
{
    public RestorePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");
    }
}
