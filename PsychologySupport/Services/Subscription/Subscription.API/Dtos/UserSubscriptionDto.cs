using Subscription.API.Data.Common;

namespace Subscription.API.Dtos;

public record UserSubscriptionDto(
    Guid Id,
    Guid PatientId,
    Guid ServicePackageId,
    Guid? PromotionCodeId,
    Guid? GiftId,
    DateTime StartDate,
    DateTime EndDate,
    SubscriptionStatus? Status
);