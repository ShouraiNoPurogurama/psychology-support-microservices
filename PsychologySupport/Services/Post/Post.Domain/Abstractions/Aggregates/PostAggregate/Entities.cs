namespace Post.Domain.Abstractions.Aggregates.PostAggregate;

public sealed class PostMedia : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid MediaId { get; set; }
    public int? Position { get; set; }
    public string? Caption { get; set; }
    public string? AltText { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    private PostMedia()
    {
    }

    public static PostMedia Create(Guid postId, Guid mediaId, int? position, string? caption = null, string? altText = null)
    {
        return new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            MediaId = mediaId,
            Position = position,
            Caption = caption,
            AltText = altText
        };
    }

    public void UpdatePosition(int newPosition)
    {
        Position = Math.Max(0, newPosition);
    }

    public void UpdateCaption(string? newCaption)
    {
        Caption = newCaption?.Trim();
    }

    public void UpdateAltText(string? newAltText)
    {
        AltText = newAltText?.Trim();
    }

    public bool HasCaption => !string.IsNullOrWhiteSpace(Caption);
    public bool HasAltText => !string.IsNullOrWhiteSpace(AltText);
}

public sealed class PostCategory : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid CategoryTagId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    private PostCategory()
    {
    }

    public static PostCategory Create(Guid postId, Guid categoryTagId)
    {
        return new PostCategory
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            CategoryTagId = categoryTagId,
            AssignedAt = DateTime.UtcNow
        };
    }
}

public sealed class PostEmotion : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid EmotionTagId { get; set; }
    public DateTime AssignedAt { get; set; }
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
            AssignedAt = DateTime.UtcNow,
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