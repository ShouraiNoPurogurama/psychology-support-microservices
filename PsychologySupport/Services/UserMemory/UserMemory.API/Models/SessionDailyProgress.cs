using BuildingBlocks.DDD;

namespace UserMemory.API.Models;

public class SessionDailyProgress: IHasCreationAudit, IHasModificationAudit
{
    public Guid AliasId { get; set; }
    public Guid SessionId { get; set; }
    public DateOnly ProgressDate { get; set; }
    public int ProgressPoints { get; set; }
    public int LastIncrement { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}