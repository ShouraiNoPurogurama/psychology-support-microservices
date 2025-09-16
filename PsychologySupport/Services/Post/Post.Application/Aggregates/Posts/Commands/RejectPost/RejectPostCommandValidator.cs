using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.RejectPost;

public sealed class RejectPostCommandValidator : AbstractValidator<RejectPostCommand>
{
    public RejectPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MaximumLength(500)
            .WithMessage("Rejection reason cannot exceed 500 characters.");
    }
}
