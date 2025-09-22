using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Domain.Abstractions
{
    public abstract class AuditableEntity<T> : IEntity<T>, IHasCreationAudit, IHasModificationAudit
    {
        [Key]
        public T Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? LastModified { get; set; }

        public string? LastModifiedBy { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not AuditableEntity<T> other) return false;

            if (ReferenceEquals(this, other)) return true;

            if (Id is null || other.Id is null) return false;

            return EqualityComparer<T>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    }
}
