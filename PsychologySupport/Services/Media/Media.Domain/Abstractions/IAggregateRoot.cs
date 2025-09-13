using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Domain.Abstractions
{
    public interface IAggregateRoot<T> : IAggregate, IEntity<T>
    {
    }

    public interface IAggregate : IDomainEventContainer, IEntity
    {
    }
}
