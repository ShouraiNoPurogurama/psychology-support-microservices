namespace BuildingBlocks.DDD;

public abstract class SoftDeletableEntity<TId> : Entity<TId>, ISoftDeletable, IHasCreationAudit
{
    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    
    public string? DeletedBy { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }


    public void MarkAsDeleted(string? deletedBy = null)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore(string subjectRef, DateTimeOffset nowUtc)
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = subjectRef;
    }
}