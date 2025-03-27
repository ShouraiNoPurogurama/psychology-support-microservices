namespace BuildingBlocks.Messaging.Events.ChatBox;

public record BookingCreatedIntegrationEvent(Guid DoctorUserId, Guid PatientUserId, Guid BookingId) 
    : IntegrationEvents;