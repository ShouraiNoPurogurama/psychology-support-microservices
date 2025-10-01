using BuildingBlocks.Enums;

namespace Subscription.API.UserSubscriptions.Dtos;

public record UpgradeUserSubscriptionDto(
    Guid PatientId,
    Guid NewServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    DateTimeOffset StartDate,
    PaymentMethodName PaymentMethodName);