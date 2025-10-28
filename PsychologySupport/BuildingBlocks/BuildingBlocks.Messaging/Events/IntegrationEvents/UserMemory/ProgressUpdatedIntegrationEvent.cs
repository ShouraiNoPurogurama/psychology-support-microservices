namespace BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;

/// <summary>
/// Published by UserMemory.API when daily progress points are updated.
/// Consumed by RealtimeHub.API to send real-time updates to the client via SignalR.
/// </summary>
public record ProgressUpdatedIntegrationEvent(
    Guid AliasId,
    Guid SessionId,
    int PointsEarned,
    int TotalSessionPoints,
    int TotalDailyPoints,
    string? Achievement,
    DateTimeOffset UpdatedAt
) : IntegrationEvent;
