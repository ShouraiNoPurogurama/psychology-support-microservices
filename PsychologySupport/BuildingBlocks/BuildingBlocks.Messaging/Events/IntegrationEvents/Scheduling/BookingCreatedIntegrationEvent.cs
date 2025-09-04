namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Scheduling;

public record BookingCreatedIntegrationEvent(Guid DoctorUserId, Guid PatientUserId, Guid BookingId) 
    : IntegrationEvent;