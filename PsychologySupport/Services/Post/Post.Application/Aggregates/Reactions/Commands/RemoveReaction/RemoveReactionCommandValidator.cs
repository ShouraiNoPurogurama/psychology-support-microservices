using FluentValidation;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

public sealed class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    public RemoveReactionCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum()
            .WithMessage("TargetType must be a valid ReactionTargetType");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("TargetId cannot be empty");
    }
}
