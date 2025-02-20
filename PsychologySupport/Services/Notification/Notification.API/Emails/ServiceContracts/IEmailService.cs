namespace Notification.API.Emails.ServiceContracts;

public interface IEmailService
{
     Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken);
}