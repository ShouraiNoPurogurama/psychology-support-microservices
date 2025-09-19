namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostCreatedWithMediaPendingIntegrationEvent(Guid PostId, string MediaOwnerType, IEnumerable<Guid> MediaIds) : IntegrationEvent;
