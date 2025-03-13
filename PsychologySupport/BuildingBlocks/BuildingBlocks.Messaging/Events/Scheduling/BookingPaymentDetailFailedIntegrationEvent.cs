namespace BuildingBlocks.Messaging.Events.Scheduling;

public record BookingPaymentDetailFailedIntegrationEvent(
    Guid BookingId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvents;