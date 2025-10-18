namespace Post.Application.ReadModels.Models;

public class UserOwnedTagReplica
{
    public Guid AliasId { get; set; }
    public Guid EmotionTagId { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; }

    // Ngày bắt đầu hiệu lực của tag
    public DateTimeOffset ValidFrom { get; set; }

    // Ngày hết hiệu lực của tag
    public DateTimeOffset ValidTo { get; set; }

}