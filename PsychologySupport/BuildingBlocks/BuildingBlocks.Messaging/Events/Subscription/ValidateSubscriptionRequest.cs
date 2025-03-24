namespace BuildingBlocks.Messaging.Events.Subscription;

public record ValidateSubscriptionRequest(
    Guid SubscriptionId,
    Guid ServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice,
    Guid PatientId,
    int DurationDays,
    decimal OldSubscriptionPrice);