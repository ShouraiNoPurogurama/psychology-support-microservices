namespace BuildingBlocks.Messaging.Events.Scheduling;

public record BookingPaymentDetailSuccessIntegrationEvent(
    Guid BookingId);