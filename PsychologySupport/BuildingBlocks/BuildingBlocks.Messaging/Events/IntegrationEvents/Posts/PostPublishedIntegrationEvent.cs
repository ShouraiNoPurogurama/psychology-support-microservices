namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostPublishedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    string Content,
    string? Title,
    DateTimeOffset PublishedAt
) : IntegrationEvent;
