namespace Post.Domain.Posts;

public partial class PostEmotion : SoftDeletableEntity<Guid>
{
    public Guid PostId { get; set; }
    public Guid EmotionTagId { get; set; }
}