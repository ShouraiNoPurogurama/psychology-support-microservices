using FluentValidation;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

public class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    private static readonly string[] ValidTargetTypes = { "post", "comment" };

    public RemoveReactionCommandValidator()
    {
        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("Vui lòng chọn loại đối tượng cần gỡ cảm xúc (Post hoặc Comment).")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Loại đối tượng chỉ được phép là: {string.Join(", ", ValidTargetTypes)}.");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn đối tượng cần gỡ cảm xúc.");
    }
}
