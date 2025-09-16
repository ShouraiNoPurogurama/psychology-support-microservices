using FluentValidation;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.UpdatePostVisibility;

public sealed class UpdatePostVisibilityCommandValidator : AbstractValidator<UpdatePostVisibilityCommand>
{
    public UpdatePostVisibilityCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Visibility must be a valid PostVisibility value.");
    }
}
