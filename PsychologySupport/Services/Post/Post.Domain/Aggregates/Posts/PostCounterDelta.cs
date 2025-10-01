namespace Post.Domain.Aggregates.Posts;

public sealed class PostCounterDelta : Entity<Guid>
{
    public Guid PostId { get; set; }
    public string CounterType { get; set; } = null!; // "reaction", "comment", "share", "view"
    public short Delta { get; set; } // +1, -1, etc.
    public DateTimeOffset OccurredAt { get; set; }
    public bool IsProcessed { get; set; }

    private PostCounterDelta()
    {
    }

    public static PostCounterDelta Create(Guid postId, string kind, short delta)
    {
        var validKinds = new[] { "reaction", "comment", "share", "view" };
        if (!validKinds.Contains(kind.ToLower()))
            throw new ArgumentException($"Invalid counter kind. Must be one of: {string.Join(", ", validKinds)}");

        return new PostCounterDelta
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            CounterType = kind.ToLower(),
            Delta = delta,
            OccurredAt = DateTimeOffset.UtcNow,
            IsProcessed = false
        };
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }

    public bool IsIncrement => Delta > 0;
    public bool IsDecrement => Delta < 0;
}
