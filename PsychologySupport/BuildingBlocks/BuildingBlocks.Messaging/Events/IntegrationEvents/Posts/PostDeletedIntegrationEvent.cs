namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostDeletedIntegrationEvent(
    Guid PostId,
    Guid DeletedByAliasId,
    DateTimeOffset DeletedAt
) : IntegrationEvent;
