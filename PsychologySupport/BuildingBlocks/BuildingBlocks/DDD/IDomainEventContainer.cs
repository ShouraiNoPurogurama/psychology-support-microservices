namespace BuildingBlocks.DDD;

public interface IDomainEventContainer
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    IDomainEvent[] ClearDomainEvents();
}
