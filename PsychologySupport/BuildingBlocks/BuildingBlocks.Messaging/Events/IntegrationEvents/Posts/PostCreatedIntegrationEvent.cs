namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostCreatedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    DateTimeOffset CreatedAt,
    int FollowerCount
) : IntegrationEvent;

