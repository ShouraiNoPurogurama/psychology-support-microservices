namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record ReactionChangedIntegrationEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string ReactionType,
    Guid AuthorAliasId,
    DateTimeOffset ChangedAt
) : IntegrationEvent;
