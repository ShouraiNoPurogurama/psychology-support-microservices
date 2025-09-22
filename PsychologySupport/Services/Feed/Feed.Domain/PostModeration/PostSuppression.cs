namespace Feed.Domain.PostModeration;

public sealed class PostSuppression
{
    public Guid PostId { get; }
    public string? Reason { get; }
    public DateTimeOffset SuppressedAt { get; }
    public DateTimeOffset? SuppressedUntil { get; }

    private PostSuppression(Guid postId, string? reason, DateTimeOffset suppressedAtUtc, DateTimeOffset? suppressedUntilUtc)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        PostId = postId;
        Reason = reason?.Trim();
        SuppressedAt = suppressedAtUtc.ToUniversalTime();
        SuppressedUntil = suppressedUntilUtc?.ToUniversalTime();
    }

    public static PostSuppression Create(Guid postId, string? reason = null, DateTimeOffset? suppressedAt = null, DateTimeOffset? suppressedUntil = null)
        => new(postId, reason, suppressedAt ?? DateTimeOffset.UtcNow, suppressedUntil);

    public bool IsCurrentlySuppressed => SuppressedUntil == null || SuppressedUntil > DateTimeOffset.UtcNow;

    public PostSuppression UpdateSuppression(string? newReason, DateTimeOffset? newSuppressedUntil)
        => new(PostId, newReason, SuppressedAt, newSuppressedUntil);
}
