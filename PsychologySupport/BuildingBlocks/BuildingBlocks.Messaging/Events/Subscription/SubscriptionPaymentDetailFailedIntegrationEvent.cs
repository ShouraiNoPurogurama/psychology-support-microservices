namespace BuildingBlocks.Messaging.Events.Subscription;

public record SubscriptionPaymentDetailFailedIntegrationEvent(
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvents;