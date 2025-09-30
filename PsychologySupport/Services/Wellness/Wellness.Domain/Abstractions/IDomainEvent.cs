using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Domain.Abstractions
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
