namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostCategoryTagsUpdatedIntegrationEvent(
    Guid PostId,
    Guid ActorAliasId,
    IEnumerable<Guid> CategoryTagIds,
    string Operation, // ATTACHED, DETACHED, UPDATED
    DateTimeOffset OccurredAt
) : IntegrationEvent;
