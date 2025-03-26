using BuildingBlocks.Enums;

namespace Subscription.API.UserSubscriptions.Dtos;

public record CreateUserSubscriptionDto(
    Guid PatientId,
    Guid ServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    DateTime StartDate,
    PaymentMethodName PaymentMethodName
);