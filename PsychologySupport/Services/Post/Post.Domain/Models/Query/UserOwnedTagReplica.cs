namespace Post.Domain.Models.Query;

public class UserOwnedTagReplica
{
    public Guid SubjectRef { get; set; }
    
    public Guid EmotionTagId { get; set; }
}