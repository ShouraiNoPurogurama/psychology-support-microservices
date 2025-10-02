namespace Post.Domain.Aggregates.Posts;

public sealed class PostEmotion : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid EmotionTagId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public double Confidence { get; set; } // AI confidence score (0-1)

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    private PostEmotion()
    {
    }

    public static PostEmotion Create(Guid postId, Guid emotionTagId, double confidence = 1.0)
    {
        return new PostEmotion
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            EmotionTagId = emotionTagId,
            AssignedAt = DateTimeOffset.UtcNow,
            Confidence = Math.Clamp(confidence, 0.0, 1.0)
        };
    }

    public void UpdateConfidence(double newConfidence)
    {
        Confidence = Math.Clamp(newConfidence, 0.0, 1.0);
    }

    public bool IsHighConfidence => Confidence >= 0.8;
    public bool IsLowConfidence => Confidence < 0.5;
}