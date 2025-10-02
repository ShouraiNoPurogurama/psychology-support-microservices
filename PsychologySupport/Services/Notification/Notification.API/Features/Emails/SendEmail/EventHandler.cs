using MediatR;
using Notification.API.Features.Emails.Contracts;
using Notification.API.Features.Emails.Models;

namespace Notification.API.Features.Emails.SendEmail;

public class SendEmailEventHandler(IEmailService emailService) : INotificationHandler<SendEmailEvent>
{
    public Task Handle(SendEmailEvent notification, CancellationToken cancellationToken)
    {
        var emailMessageDto = CreateEmailMessageDto(notification);

        return emailService.SendEmailAsync(emailMessageDto, cancellationToken);
    }

    private EmailMessageDto CreateEmailMessageDto(SendEmailEvent notification)
        => new EmailMessageDto(
            notification.EventId,
            notification.To,
            notification.Subject,
            notification.Body
        );
}