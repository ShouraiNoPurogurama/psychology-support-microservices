namespace Subscription.API.UserSubscriptions.Dtos;

public record GetUserSubscriptionDto(
    Guid Id,
    Guid PatientId,
    Guid ServicePackageId,
    DateTime StartDate,
    DateTime EndDate,
    Guid? PromotionId,
    Guid? GiftId,
    string Status
);