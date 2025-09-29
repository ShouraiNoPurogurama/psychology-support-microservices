using System.ComponentModel.DataAnnotations;

namespace Conversation.Domain.Abstractions;

public abstract class Entity<T> : IEntity<T>
{
    [Key]
    public T Id { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<T> other) return false;
        
        if(ReferenceEquals(this, other)) return true;
        
        if(Id is null || other.Id is null) return false;

        return EqualityComparer<T>.Default.Equals(Id, other.Id);
    }
    
    public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    public static bool operator ==(Entity<T>? a, Entity<T>? b) =>
        a is null && b is null || a is not null && a.Equals(b);

    public static bool operator !=(Entity<T>? a, Entity<T>? b) => !(a == b);
}

public abstract class AuditableEntity<T> : Entity<T>
{
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IEntity<T>
{
    T Id { get; set; }
}

public interface IAggregateRoot<T> : IEntity<T>
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    IDomainEvent[] ClearDomainEvents();
}

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOn { get; }
}

public abstract record DomainEvent(Guid AggregateId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
