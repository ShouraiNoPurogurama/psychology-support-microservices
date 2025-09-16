namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostMediaUpdatedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    string PostMediaUpdateStatus,
    DateTimeOffset UtcNow) : IntegrationEvent;