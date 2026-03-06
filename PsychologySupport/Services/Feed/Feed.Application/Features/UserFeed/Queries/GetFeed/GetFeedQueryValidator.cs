using Feed.Application.Abstractions.CursorService;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Feed.Application.Features.UserFeed.Queries.GetFeed;

public sealed class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator(ICursorService cursorService, ILogger<GetFeedQueryValidator> logger)
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
            .Must(cursor => BeValidSignedCursor(cursor, cursorService, logger))
            .WithMessage("Invalid or tampered pagination cursor.")
            .When(x => !string.IsNullOrEmpty(x.Cursor));
    }

    private static bool BeValidSignedCursor(string? cursor, ICursorService cursorService, ILogger logger)
    {
        if (string.IsNullOrEmpty(cursor)) return true;

        var isValid = cursorService.ValidateCursor(cursor);

        if (!isValid)
        {
            logger.LogWarning("Invalid cursor detected. Cursor value (truncated): {Cursor}",
                cursor.Length > 20 ? cursor[..20] + "…" : cursor);
        }

        return isValid;
    }
}
