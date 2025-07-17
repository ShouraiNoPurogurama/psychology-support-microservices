namespace Notification.API.Emails.ServiceContracts;

public interface IEmailService
{
     Task<bool> HasSentEmailRecentlyAsync(string email, CancellationToken cancellationToken);
     Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken);
}