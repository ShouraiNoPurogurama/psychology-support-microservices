using FluentValidation;

namespace Post.Application.Features.Posts.Commands.UpdateMediaAltText;

public sealed class UpdateMediaAltTextCommandValidator : AbstractValidator<UpdateMediaAltTextCommand>
{
    public UpdateMediaAltTextCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("MediaId is required.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.AltText)
            .NotEmpty()
            .WithMessage("AltText is required.")
            .MaximumLength(500)
            .WithMessage("AltText cannot exceed 500 characters.");
    }
}
