namespace Post.Application.ReadModels;

public class UserOwnedTagReplica
{
    public Guid SubjectRef { get; set; }
    
    public Guid EmotionTagId { get; set; }
}