namespace Post.Application.ReadModels.Models;

public class UserOwnedTagReplica
{
    public Guid SubjectRef { get; set; }
    public Guid EmotionTagId { get; set; }
    
    public DateTimeOffset LastSyncedAt { get; set; }

}