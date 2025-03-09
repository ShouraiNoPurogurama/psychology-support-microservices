using BuildingBlocks.Enums;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Dtos;

public record BuySubscriptionDto(
    Guid SubscriptionId,
    Guid ServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice,
    Guid PatientId,
    string PatientEmail,
    int DurationDays,
    PaymentMethodName PaymentMethod,
    PaymentType PaymentType
    ) : BasePaymentDto(FinalPrice, PatientId, PaymentMethod);