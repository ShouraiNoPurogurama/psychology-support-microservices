namespace Notification.API.Models.Notifications;

public class ProcessedIntegrationEvent : AuditableEntity<Guid>
{
    // Id is the MessageId from MassTransit
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; }

    public static ProcessedIntegrationEvent Create(Guid messageId, string eventType)
    {
        return new ProcessedIntegrationEvent
        {
            Id = messageId,
            EventType = eventType,
            ReceivedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
