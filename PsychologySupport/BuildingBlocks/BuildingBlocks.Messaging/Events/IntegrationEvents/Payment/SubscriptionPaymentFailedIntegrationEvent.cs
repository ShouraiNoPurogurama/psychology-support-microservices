namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record SubscriptionPaymentFailedIntegrationEvent(
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvent;