using FluentValidation;

namespace Post.Application.Features.Reactions.Queries.GetReactedPosts;

public class GetReactedPostsQueryValidator : AbstractValidator<GetReactedPostsQuery>
{
    public GetReactedPostsQueryValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("AliasId is required");

        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("PageIndex must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.ReactionCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.ReactionCode))
            .WithMessage("ReactionCode must not exceed 50 characters");
    }
}
