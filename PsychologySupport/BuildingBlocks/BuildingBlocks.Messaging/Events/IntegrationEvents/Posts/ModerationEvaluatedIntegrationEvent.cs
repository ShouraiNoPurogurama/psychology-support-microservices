namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public enum ModerationDecision { Approved, Rejected }

public record ModerationEvaluatedIntegrationEvent(
    Guid PostId,
    ModerationDecision Decision,
    string? Reason
) : IntegrationEvent;

