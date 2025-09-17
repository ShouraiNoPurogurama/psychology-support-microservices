using FluentValidation;

namespace Post.Application.Features.Posts.Queries.GetPostsByTag;

public sealed class GetPostsByTagQueryValidator : AbstractValidator<GetPostsByTagQuery>
{
    public GetPostsByTagQueryValidator()
    {
        RuleFor(x => x.CategoryTagId)
            .NotEmpty()
            .WithMessage("CategoryTagId is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Size must be between 1 and 100.");
    }
}
