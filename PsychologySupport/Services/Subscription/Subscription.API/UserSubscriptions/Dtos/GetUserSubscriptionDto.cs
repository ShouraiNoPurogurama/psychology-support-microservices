using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Dtos;

public record GetUserSubscriptionDto(
    Guid Id,
    Guid PatientId,
    Guid ServicePackageId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    Guid? PromotionId,
    Guid? GiftId,
    SubscriptionStatus Status
);