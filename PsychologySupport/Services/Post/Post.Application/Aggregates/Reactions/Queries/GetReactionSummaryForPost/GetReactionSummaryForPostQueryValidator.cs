using FluentValidation;

namespace Post.Application.Aggregates.Reactions.Queries.GetReactionSummaryForPost;

public sealed class GetReactionSummaryForPostQueryValidator : AbstractValidator<GetReactionSummaryForPostQuery>
{
    public GetReactionSummaryForPostQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");
    }
}
