using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Domain.Abstractions
{
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
}
