using FluentValidation;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

public class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    private static readonly string[] ValidTargetTypes = { "post", "comment" };

    public RemoveReactionCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("Target type is required")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Target type must be one of: {string.Join(", ", ValidTargetTypes)}");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Target ID is required");
    }
}
