namespace Billing.API.Abstractions
{
    public interface IDomainEventContainer
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        IDomainEvent[] ClearDomainEvents();
    }
}
