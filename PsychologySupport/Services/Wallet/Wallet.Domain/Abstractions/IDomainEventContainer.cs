namespace Wallet.Domain.Abstractions
{
    public interface IDomainEventContainer
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        IDomainEvent[] ClearDomainEvents();
    }
}
