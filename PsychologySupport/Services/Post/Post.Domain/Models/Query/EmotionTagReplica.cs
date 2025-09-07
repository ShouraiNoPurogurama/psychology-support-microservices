namespace Post.Domain.Models.Query;

public class EmotionTagReplica : Entity<Guid>
{
    public string Code { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public Guid? MediaId { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; }
}