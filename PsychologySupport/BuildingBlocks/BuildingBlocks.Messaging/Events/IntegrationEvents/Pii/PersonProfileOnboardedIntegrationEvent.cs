namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;

public record PersonProfileOnboardedIntegrationEvent(Guid UserId) : IntegrationEvent;