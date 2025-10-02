namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record CommentDeletedIntegrationEvent(
    Guid CommentId,
    Guid PostId,
    Guid DeletedByAliasId,
    DateTimeOffset DeletedAt
) : IntegrationEvent;
