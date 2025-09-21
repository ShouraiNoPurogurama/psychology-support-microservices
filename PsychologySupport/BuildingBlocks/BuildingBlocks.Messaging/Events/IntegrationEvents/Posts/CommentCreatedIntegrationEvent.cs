namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record CommentCreatedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId
) : IntegrationEvent;

