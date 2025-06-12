using BuildingBlocks.Enums;

namespace Payment.Application.Payments.Dtos;

public record BuySubscriptionDto(
    Guid SubscriptionId,
    Guid ServicePackageId,
    Guid PatientId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice,
    int DurationDays,
    PaymentMethodName PaymentMethod,
    PaymentType PaymentType,
    string ServicePackageName
) : BasePaymentDto(FinalPrice, PatientId, PaymentMethod);