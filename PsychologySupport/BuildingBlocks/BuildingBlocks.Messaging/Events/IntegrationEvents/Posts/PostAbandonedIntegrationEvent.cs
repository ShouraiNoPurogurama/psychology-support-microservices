namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostAbandonedIntegrationEvent(
    Guid PostId,
    string Content,
    Guid AuthorAliasId,
    DateTimeOffset AbandonedAt
) : IntegrationEvent;
