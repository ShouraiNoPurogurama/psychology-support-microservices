namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Media;

public record MediaAttachedToOwnerIntegrationEvent(Guid OwnerId, string OwnerType, List<Guid> SucceededMediaIds) : IntegrationEvent;