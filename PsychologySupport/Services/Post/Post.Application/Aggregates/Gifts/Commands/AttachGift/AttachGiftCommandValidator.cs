using FluentValidation;

namespace Post.Application.Aggregates.Gifts.Commands.AttachGift;

public class AttachGiftCommandValidator : AbstractValidator<AttachGiftCommand>
{
    private static readonly string[] ValidTargetTypes = { "post", "comment" };

    public AttachGiftCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("Target type is required")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Target type must be one of: {string.Join(", ", ValidTargetTypes)}");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Target ID is required");

        RuleFor(x => x.GiftId)
            .NotEmpty()
            .WithMessage("Gift ID is required");

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .WithMessage("Gift message cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}
