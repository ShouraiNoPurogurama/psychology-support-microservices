namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostRestoredIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    DateTimeOffset RestoredAt
) : IntegrationEvent;
