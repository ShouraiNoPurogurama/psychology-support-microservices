namespace Notification.API.Emails.Dtos;

public record EmailMessageDto(Guid MessageId, string To, string Subject, string Body);