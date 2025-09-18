using FluentValidation;

namespace Post.Application.Features.Reactions.Commands.CreateReaction;

public sealed class CreateReactionCommandValidator : AbstractValidator<CreateReactionCommand>
{
    public CreateReactionCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum()
            .WithMessage("TargetType must be a valid ReactionTargetType");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("TargetId cannot be empty");

        RuleFor(x => x.ReactionCode)
            .IsInEnum()
            .WithMessage("ReactionCode must be a valid ReactionCode");
    }
}
