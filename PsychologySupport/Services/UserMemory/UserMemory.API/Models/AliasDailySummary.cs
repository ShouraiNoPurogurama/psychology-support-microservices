namespace UserMemory.API.Models;

using BuildingBlocks.DDD;

public class AliasDailySummary : IHasCreationAudit, IHasModificationAudit
{
    public Guid AliasId { get; set; }
    public DateOnly Date { get; set; }
    
    // Chỉ dùng để đếm số lần đã đổi thưởng trong ngày (Điều kiện 2)
    public int RewardClaimCount { get; set; } 
    
    // Audit fields
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}