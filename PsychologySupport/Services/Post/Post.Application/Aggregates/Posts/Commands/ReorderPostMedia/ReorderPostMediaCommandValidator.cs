using FluentValidation;

namespace Post.Application.Aggregates.Posts.Commands.ReorderPostMedia;

public sealed class ReorderPostMediaCommandValidator : AbstractValidator<ReorderPostMediaCommand>
{
    public ReorderPostMediaCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.OrderedMediaIds)
            .NotNull()
            .WithMessage("OrderedMediaIds is required.")
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one media ID must be provided.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate media IDs are not allowed.");
    }
}
