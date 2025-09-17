using FluentValidation;

namespace Post.Application.Features.Reactions.Queries.GetReactionsForPost;

public sealed class GetReactionsForPostQueryValidator : AbstractValidator<GetReactionsForPostQuery>
{
    public GetReactionsForPostQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("PostId is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Size must be between 1 and 100.");
    }
}
