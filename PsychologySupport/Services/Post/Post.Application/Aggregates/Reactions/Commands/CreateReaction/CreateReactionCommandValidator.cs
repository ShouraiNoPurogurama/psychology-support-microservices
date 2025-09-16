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
            .WithMessage("Vui lòng chọn loại đối tượng cần tương tác (Post hoặc Comment).")
            .Must(type => ValidTargetTypes.Contains(type.ToLower()))
            .WithMessage($"Loại đối tượng chỉ được phép là: {string.Join(", ", ValidTargetTypes)}.");

        RuleFor(x => x.TargetId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn đối tượng cần tương tác.");

        RuleFor(x => x.ReactionCode)
            .NotEmpty()
            .WithMessage("Vui lòng chọn loại cảm xúc.")
            .Must(code => ValidReactionCodes.Contains(code.ToLower()))
            .WithMessage($"Loại cảm xúc chỉ được phép là: {string.Join(", ", ValidReactionCodes)}.");
    }
}
