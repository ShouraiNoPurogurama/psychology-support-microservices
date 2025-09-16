namespace Post.Domain.Abstractions;

public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}

public interface IEntity;


public interface IHasCreationAudit
{
    DateTimeOffset CreatedAt { get; set; }
    Guid CreatedByAliasId { get; set; }
}

public interface IHasModificationAudit
{
    DateTimeOffset? LastModified { get; set; }
    string? LastModifiedByAliasId { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedByAliasId { get; set; }
}

public interface IHasRowVersion
{
    byte[] RowVersion { get; set; }
}