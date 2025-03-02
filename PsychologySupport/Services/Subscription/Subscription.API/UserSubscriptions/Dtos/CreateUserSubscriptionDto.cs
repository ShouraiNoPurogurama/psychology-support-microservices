using BuildingBlocks.Enums;

namespace Subscription.API.UserSubscriptions.Dtos;

public record CreateUserSubscriptionDto(
    Guid PatientId,
    Guid ServicePackageId,
    Guid? PromotionCodeId,
    Guid? GiftId,
    DateTime StartDate,
    DateTime EndDate,
    PaymentMethodName PaymentMethodName
);