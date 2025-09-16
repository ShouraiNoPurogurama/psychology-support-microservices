namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostMediaUpdatedIntegrationEvent(
    Guid PostId,
    Guid ActorAliasId,
    IEnumerable<Guid> MediaIds,
    string Operation, // ATTACHED, REMOVED, UPDATED
    DateTimeOffset OccurredAt
) : IntegrationEvent;
