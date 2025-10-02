using FluentValidation;

namespace Notification.API.Features.Notifications.Commands.MarkRead;

public class MarkNotificationsReadCommandValidator : AbstractValidator<MarkNotificationsReadCommand>
{
    public MarkNotificationsReadCommandValidator()
    {
        RuleFor(x => x.RecipientAliasId)
            .NotEmpty()
            .WithMessage("RecipientAliasId is required");

        RuleFor(x => x.NotificationIds)
            .NotEmpty()
            .WithMessage("NotificationIds cannot be empty");

        RuleForEach(x => x.NotificationIds)
            .NotEmpty()
            .WithMessage("NotificationId cannot be empty");
    }
}
