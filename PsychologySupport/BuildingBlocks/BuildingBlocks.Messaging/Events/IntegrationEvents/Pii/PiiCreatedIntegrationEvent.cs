namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;

public record PiiCreatedIntegrationEvent(Guid SubjectRef) : IntegrationEvent;