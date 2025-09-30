using Notification.API.Features.Emails.Models;

namespace Notification.API.Features.Emails.Contracts;

public interface IEmailService
{
     Task<bool> HasSentEmailRecentlyAsync(string email, CancellationToken cancellationToken);
     Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken);
}