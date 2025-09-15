using FluentValidation;

namespace Post.Application.Aggregates.Reactions.Commands.CreateReaction;

public class CreateReactionCommandValidator : AbstractValidator<CreateReactionCommand>
{
    private static readonly string[] ValidTargetTypes = { "post", "comment" };
    private static readonly string[] ValidReactionCodes = { "like", "heart", "laugh", "wow", "sad", "angry" };

    public CreateReactionCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("Target type is required")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Target type must be one of: {string.Join(", ", ValidTargetTypes)}");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Target ID is required");

        RuleFor(x => x.ReactionCode)
            .NotEmpty()
            .WithMessage("Reaction code is required")
            .Must(code => ValidReactionCodes.Contains(code.ToLower()))
            .WithMessage($"Reaction code must be one of: {string.Join(", ", ValidReactionCodes)}");
    }
}
