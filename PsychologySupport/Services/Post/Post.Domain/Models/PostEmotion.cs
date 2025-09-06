namespace Post.Domain.Models;

public partial class PostEmotion
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid EmotionTagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedByAliasId { get; set; }

    public DateTime? DeletedAt { get; set; }
}
