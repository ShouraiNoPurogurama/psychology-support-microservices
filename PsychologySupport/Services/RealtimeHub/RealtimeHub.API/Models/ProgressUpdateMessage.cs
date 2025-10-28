namespace RealtimeHub.API.Models;

/// <summary>
/// Progress update message for real-time delivery to clients.
/// Sent when user earns progress points from messages.
/// </summary>
public record ProgressUpdateMessage
{
    public Guid AliasId { get; init; }
    public Guid SessionId { get; init; }
    public int PointsEarned { get; init; }
    public int TotalSessionPoints { get; init; }
    public int TotalDailyPoints { get; init; }
    public string? Achievement { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
