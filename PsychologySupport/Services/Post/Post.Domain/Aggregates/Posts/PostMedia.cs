namespace Post.Domain.Aggregates.Posts;

public sealed class PostMedia : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid MediaId { get; set; }
    public int? Position { get; set; }
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    public bool IsCover { get; set; } = false;

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
            AltText = altText,
            IsCover = false
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