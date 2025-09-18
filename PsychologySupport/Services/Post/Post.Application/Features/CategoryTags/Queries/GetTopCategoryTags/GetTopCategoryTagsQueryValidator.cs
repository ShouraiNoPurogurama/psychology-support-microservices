using FluentValidation;

namespace Post.Application.Features.CategoryTags.Queries.GetTopCategoryTags;

public sealed class GetTopCategoryTagsQueryValidator : AbstractValidator<GetTopCategoryTagsQuery>
{
    public GetTopCategoryTagsQueryValidator()
    {
        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Size must be between 1 and 100.");
    }
}
