namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostUnpublishedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    DateTimeOffset UnpublishedAt
) : IntegrationEvent;
