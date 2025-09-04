namespace BuildingBlocks.Messaging.Events.Queries.Subscription;

public record ValidateSubscriptionRequest(
    Guid SubscriptionId,
    Guid ServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice,
    Guid PatientId,
    int DurationDays,
    decimal OldSubscriptionPrice);