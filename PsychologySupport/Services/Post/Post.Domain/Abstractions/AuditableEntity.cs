using System.ComponentModel.DataAnnotations;

namespace Post.Domain.Abstractions;

public abstract class AuditableEntity<T> : IEntity<T>, IHasCreationAudit, IHasModificationAudit
{
    [Key]
    public T Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid CreatedByAliasId { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedByAliasId { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not AuditableEntity<T> other) return false;

        if (ReferenceEquals(this, other)) return true;

        if (Id is null || other.Id is null) return false;

        return EqualityComparer<T>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}