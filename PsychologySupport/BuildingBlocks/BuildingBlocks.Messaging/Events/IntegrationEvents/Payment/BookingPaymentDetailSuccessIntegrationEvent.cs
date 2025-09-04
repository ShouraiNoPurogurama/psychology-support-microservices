namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record BookingPaymentDetailSuccessIntegrationEvent(
    Guid BookingId);