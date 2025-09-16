using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.PublishPost;

public sealed class PublishPostCommandValidator : AbstractValidator<PublishPostCommand>
{
    public PublishPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");
    }
}
