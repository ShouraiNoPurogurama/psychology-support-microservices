namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record ReactionAddedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId
) : IntegrationEvent;

