namespace Post.Application.ReadModels.Models;

public class AliasVersionReplica
{
    public Guid AliasId { get; set; }               
    public Guid CurrentVersionId { get; set; }
    public string Label { get; set; } = null!;
    
    public DateTimeOffset ValidFrom { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
