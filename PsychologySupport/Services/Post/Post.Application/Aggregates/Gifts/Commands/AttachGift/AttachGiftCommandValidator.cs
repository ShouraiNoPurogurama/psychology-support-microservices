using FluentValidation;
using Post.Domain.Aggregates.Gifts.Enums;

namespace Post.Application.Aggregates.Gifts.Commands.AttachGift;

public sealed class AttachGiftCommandValidator : AbstractValidator<AttachGiftCommand>
{
    public AttachGiftCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum()
            .WithMessage("TargetType must be a valid GiftTargetType");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("TargetId cannot be empty");

        RuleFor(x => x.GiftId)
            .NotEmpty()
            .WithMessage("GiftId cannot be empty");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Message))
            .WithMessage("Message cannot exceed 500 characters");
    }
}
