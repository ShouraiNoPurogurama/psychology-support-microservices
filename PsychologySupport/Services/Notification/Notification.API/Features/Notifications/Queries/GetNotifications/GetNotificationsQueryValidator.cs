using FluentValidation;
using Notification.API.Common;

namespace Notification.API.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.RecipientAliasId)
            .NotEmpty()
            .WithMessage("RecipientAliasId is required");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Cursor)
            .Must(BeValidCursor)
            .When(x => !string.IsNullOrEmpty(x.Cursor))
            .WithMessage("Invalid cursor format");
    }

    private bool BeValidCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return true;
        return NotificationCursor.Decode(cursor) != null;
    }
}
