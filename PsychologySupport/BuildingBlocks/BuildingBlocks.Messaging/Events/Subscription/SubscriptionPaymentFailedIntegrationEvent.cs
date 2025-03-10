namespace BuildingBlocks.Messaging.Events.Subscription;

public record SubscriptionPaymentFailedIntegrationEvent(
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvents;