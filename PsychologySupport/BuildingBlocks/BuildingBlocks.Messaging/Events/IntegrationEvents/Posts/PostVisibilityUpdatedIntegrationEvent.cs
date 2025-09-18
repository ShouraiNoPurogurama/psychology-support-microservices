namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostVisibilityUpdatedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    string OldVisibility,
    string NewVisibility,
    DateTimeOffset UpdatedAt
) : IntegrationEvent;
