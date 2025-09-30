namespace Notification.API.Features.Emails.Models;

public record EmailMessageDto(Guid MessageId, string To, string Subject, string Body);