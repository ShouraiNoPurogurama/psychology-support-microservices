namespace RealtimeHub.API.Models;

/// <summary>
/// Notification message model for real-time delivery
/// </summary>
public record NotificationMessage
{
    public Guid Id { get; init; }
    public Guid RecipientAliasId { get; init; }
    public Guid? ActorAliasId { get; init; }
    public string ActorDisplayName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public Guid? PostId { get; init; }
    public Guid? CommentId { get; init; }
    public Guid? ReactionId { get; init; }
    public Guid? FollowId { get; init; }
    public string? ModerationAction { get; init; }
    public string? Snippet { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
