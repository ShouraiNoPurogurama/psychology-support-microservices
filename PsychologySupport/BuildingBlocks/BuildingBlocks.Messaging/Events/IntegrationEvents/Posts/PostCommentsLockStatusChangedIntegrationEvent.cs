namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostCommentsLockStatusChangedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    bool IsCommentsLocked,
    DateTimeOffset UpdatedAt
) : IntegrationEvent;
