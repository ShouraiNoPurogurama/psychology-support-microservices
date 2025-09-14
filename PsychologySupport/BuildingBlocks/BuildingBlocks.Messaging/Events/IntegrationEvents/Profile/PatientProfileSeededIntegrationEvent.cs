namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;

public record PatientProfileSeededIntegrationEvent(Guid SubjectRef, Guid ProfileId) : IntegrationEvent;