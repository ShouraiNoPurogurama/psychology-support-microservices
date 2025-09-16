using FluentValidation;

namespace Post.Application.Aggregates.Gifts.Commands.AttachGift;

public class AttachGiftCommandValidator : AbstractValidator<AttachGiftCommand>
{
    private static readonly string[] ValidTargetTypes = { "post", "comment" };

    public AttachGiftCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("Vui lòng chọn loại đối tượng nhận quà (Post hoặc Comment).")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Loại đối tượng chỉ được phép là: {string.Join(", ", ValidTargetTypes)}.");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn đối tượng nhận quà.");

        RuleFor(x => x.GiftId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn loại quà tặng.");

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .WithMessage("Lời nhắn không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}
