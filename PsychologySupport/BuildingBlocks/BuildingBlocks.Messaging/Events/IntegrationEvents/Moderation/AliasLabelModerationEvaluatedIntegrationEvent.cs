namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Moderation;

/// <summary>
/// Event raised by AIModeration service when an alias label has been evaluated
/// </summary>
public record AliasLabelModerationEvaluatedIntegrationEvent(
    string Label,
    bool IsValid,
    List<string> Reasons,
    string PolicyVersion,
    DateTimeOffset EvaluatedAt
) : IntegrationEvent;
