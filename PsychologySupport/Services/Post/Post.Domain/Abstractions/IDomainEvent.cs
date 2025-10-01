using MediatR;

namespace Post.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    Guid EventId => Guid.NewGuid();
    
    public DateTimeOffset OccurredOn => DateTimeOffset.Now;
    
    public string EventType => GetType().AssemblyQualifiedName!;
}

public abstract record DomainEvent(Guid EventId) : IDomainEvent
{
    public DomainEvent() : this(Guid.NewGuid()) { }

    public DateTimeOffset OccurredOn => DateTimeOffset.Now;
    
    public string EventType => GetType().AssemblyQualifiedName!;
}
