namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record UpgradeSubscriptionPaymentFailedIntegrationEvent(
    Guid SubjectRef,
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IntegrationEvent;