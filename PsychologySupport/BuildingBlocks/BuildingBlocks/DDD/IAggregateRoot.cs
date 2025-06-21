namespace BuildingBlocks.DDD;


public interface IAggregateRoot<T> : IAggregate, IEntity<T>
{
}

public interface IAggregate : IDomainEventContainer, IEntity
{
}