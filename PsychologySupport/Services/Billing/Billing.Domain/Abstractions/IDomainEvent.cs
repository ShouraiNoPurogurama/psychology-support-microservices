using MediatR;

namespace Billing.Domain.Abstractions
{
    public interface IDomainEvent : INotification
    {
        Guid EventId => Guid.NewGuid();

        public DateTime OccurredOn => DateTime.Now;

        public string EventType => GetType().AssemblyQualifiedName!;
    }

    public abstract record DomainEvent(Guid EventId) : IDomainEvent
    {
        public DomainEvent() : this(Guid.NewGuid()) { }

        public DateTime OccurredOn => DateTime.Now;

        public string EventType => GetType().AssemblyQualifiedName!;
    }
}
