using Subscription.API.Data.Common;

namespace Subscription.API.Dtos;

public record GetUserSubscriptionDto(
    Guid Id,
    Guid PatientId,
    Guid ServicePackageId,
    DateTime StartDate,
    DateTime EndDate,
    Guid? PromotionId,
    Guid? GiftId,
    SubscriptionStatus Status
);