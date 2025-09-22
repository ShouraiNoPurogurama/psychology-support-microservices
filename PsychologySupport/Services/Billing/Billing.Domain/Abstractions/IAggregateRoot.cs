namespace Billing.Domain.Abstractions
{
    public interface IAggregateRoot<T> : IAggregate, IEntity<T>
    {
    }

    public interface IAggregate : IDomainEventContainer, IEntity
    {
    }
}
