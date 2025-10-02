using FluentValidation;

namespace Post.Application.Features.Posts.Commands.AbandonPost;

public class AbandonPostCommandValidator : AbstractValidator<AbandonPostCommand>
{
    public AbandonPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");
    }
}
