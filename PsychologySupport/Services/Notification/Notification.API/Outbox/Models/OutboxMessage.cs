using System.Text.Json;
using BuildingBlocks.Messaging.Events;

namespace Notification.API.Outbox.Models;

public class OutboxMessage : Entity<Guid>
{
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTimeOffset OccuredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }

    public void Process()
    {
        ProcessedOn = DateTimeOffset.UtcNow.AddHours(7);
    }

    public static OutboxMessage Create<TMessage>(TMessage eventMessage) 
        where TMessage : class
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TMessage).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(eventMessage),
            OccuredOn = DateTimeOffset.UtcNow.AddHours(7)
        };
    }
}