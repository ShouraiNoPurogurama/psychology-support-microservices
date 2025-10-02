namespace BuildingBlocks.Messaging.Events.IntegrationEvents;

public record IntegrationEvent
{
    public Guid Id => Guid.NewGuid();

    public DateTimeOffset OccurredOn => DateTimeOffset.Now;

    public string EventType => GetType().AssemblyQualifiedName!;
}