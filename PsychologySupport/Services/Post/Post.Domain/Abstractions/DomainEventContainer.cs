namespace Post.Domain.Abstractions;

public class DomainEventContainer : IDomainEventContainer
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IDomainEvent[] ClearDomainEvents()
    {
        var dequeued = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeued;
    }
}
