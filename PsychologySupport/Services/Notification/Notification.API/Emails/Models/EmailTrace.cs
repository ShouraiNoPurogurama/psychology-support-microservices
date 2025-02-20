using Notification.API.Emails.ValueObjects;

namespace Notification.API.Emails.Models;

public class EmailTrace
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public required string To { get; set; }
    
    public required string Subject { get; set; }
    
    public required string Body { get; set; }
    
    public required EmailTraceStatus Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public required string TrackerId { get; set; }
    
    public static EmailTrace Create(
        string to,
        string subject,
        string body,
        Guid messageId,
        string trackId) => new ()
    {
        To = to,
        Body = body,
        Status = EmailTraceStatus.Sent,
        CreatedAt = DateTimeOffset.UtcNow,
        MessageId = messageId,
        Subject = subject,
        TrackerId = trackId
    };
}