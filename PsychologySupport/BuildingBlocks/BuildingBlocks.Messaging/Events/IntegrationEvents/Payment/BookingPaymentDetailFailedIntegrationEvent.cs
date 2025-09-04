namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record BookingPaymentDetailFailedIntegrationEvent(
    Guid BookingId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvent;