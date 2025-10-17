namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Moderation;

/// <summary>
/// Event raised by AIModeration service when a post content has been evaluated
/// </summary>
public record PostModerationEvaluatedIntegrationEvent(
    Guid PostId,
    string Status, // Approved, Rejected, Flagged
    List<string> Reasons,
    string PolicyVersion,
    DateTimeOffset EvaluatedAt
) : IntegrationEvent;
