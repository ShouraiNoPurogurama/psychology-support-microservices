namespace Post.Application.ReadModels.Models;

public class UserOwnedTagReplica
{
    public Guid AliasId { get; set; }
    public Guid EmotionTagId { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; }

    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset ValidTo { get; set; }

}