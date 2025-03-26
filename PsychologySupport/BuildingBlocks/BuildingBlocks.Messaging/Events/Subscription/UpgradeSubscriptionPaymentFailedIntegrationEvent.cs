namespace BuildingBlocks.Messaging.Events.Subscription;

public record UpgradeSubscriptionPaymentFailedIntegrationEvent(
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvents;