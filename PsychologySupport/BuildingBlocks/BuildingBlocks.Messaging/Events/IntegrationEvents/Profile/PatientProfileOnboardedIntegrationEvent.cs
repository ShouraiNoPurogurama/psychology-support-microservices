namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;

public record PatientProfileOnboardedIntegrationEvent(Guid UserId) : IntegrationEvent;