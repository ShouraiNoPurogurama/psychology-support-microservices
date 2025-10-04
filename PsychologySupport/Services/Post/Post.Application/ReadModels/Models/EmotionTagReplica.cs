namespace Post.Application.ReadModels.Models;

public class EmotionTagReplica
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public Guid? MediaId { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; }

    public string? UnicodeCodepoint { get; private set; }
}