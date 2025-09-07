namespace BuildingBlocks.DDD;

public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}

public interface IEntity;


public interface IHasCreationAudit
{
    DateTimeOffset? CreatedAt { get; set; }
    string? CreatedBy { get; set; }
}

public interface IHasModificationAudit
{
    DateTimeOffset? LastModified { get; set; }
    string? LastModifiedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

public interface IHasRowVersion
{
    byte[] RowVersion { get; set; }
}