namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record ReactionAddedIntegrationEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    Guid TargetAuthorAliasId,
    Guid ReactorAliasId,
    string ReactorLabel,
    string ReactionCode,
    DateTimeOffset ReactedAt
) : IntegrationEvent;

