using System.ComponentModel.DataAnnotations;

namespace Wellness.Domain.Abstractions
{
    public abstract class Entity<T> : IEntity<T>
    {
        [Key]
        public T Id { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<T> other) return false;

            if (ReferenceEquals(this, other)) return true;

            if (Id is null || other.Id is null) return false;

            return EqualityComparer<T>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

        public static bool operator ==(Entity<T>? a, Entity<T>? b) =>
            a is null && b is null || a is not null && a.Equals(b);

        public static bool operator !=(Entity<T>? a, Entity<T>? b) => !(a == b);

    }
}
