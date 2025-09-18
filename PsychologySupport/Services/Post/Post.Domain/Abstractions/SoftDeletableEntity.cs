namespace Post.Domain.Abstractions;

public abstract class SoftDeletableEntity<TId> : Entity<TId>, ISoftDeletable, IHasCreationAudit
{
    public bool IsDeleted { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }
    
    public string? DeletedByAliasId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public Guid CreatedByAliasId { get; set; }
    
    public void MarkAsDeleted(string? deletedByAliasId = null)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deletedByAliasId;
    }
    
    public void Restore(string aliasId, DateTimeOffset nowUtc)
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = aliasId; 
    }
}