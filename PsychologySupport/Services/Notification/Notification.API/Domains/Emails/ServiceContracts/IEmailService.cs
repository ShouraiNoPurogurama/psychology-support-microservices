using Notification.API.Domains.Emails.Dtos;

namespace Notification.API.Domains.Emails.ServiceContracts;

public interface IEmailService
{
     Task<bool> HasSentEmailRecentlyAsync(string email, CancellationToken cancellationToken);
     Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken);
}