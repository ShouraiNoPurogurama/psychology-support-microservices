using FluentValidation;

namespace Post.Application.Features.Reactions.Queries.GetUserReactionForPost;

public sealed class GetUserReactionForPostQueryValidator : AbstractValidator<GetUserReactionForPostQuery>
{
    public GetUserReactionForPostQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("AliasId is required.");
    }
}
