using FluentValidation;

namespace Feed.Application.Features.UserFeed.Queries.GetFeed;

public sealed class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty()
            .WithMessage("AliasId is required");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PageIndex must be non-negative");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Cursor)
            .Must(BeValidCursor)
            .WithMessage("Invalid cursor format")
            .When(x => !string.IsNullOrEmpty(x.Cursor));
    }

    private static bool BeValidCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return true;
        
        try
        {
            // Basic base64 validation
            Convert.FromBase64String(cursor);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
