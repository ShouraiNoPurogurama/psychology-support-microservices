using FluentValidation;

namespace Post.Application.Features.Posts.Commands.ApprovePost;

public sealed class ApprovePostCommandValidator : AbstractValidator<ApprovePostCommand>
{
    public ApprovePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");
    }
}
