namespace BuildingBlocks.Messaging.Events.IntegrationEvents;

public record IntegrationEvent
{
    public Guid Id => Guid.NewGuid();

    public DateTime OccurredOn => DateTime.Now;

    public string EventType => GetType().AssemblyQualifiedName!;
}