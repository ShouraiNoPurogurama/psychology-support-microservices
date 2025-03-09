using BuildingBlocks.Enums;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Dtos;

public record BuySubscriptionDto(
    Guid ServicePackageId,
    Guid? PromotionCodeId,
    Guid? GiftId,
    decimal TotalAmount,
    Guid PatientId,
    int DurationDays,
    PaymentMethodName PaymentMethod,
    PaymentType PaymentType
    ) : BasePaymentDto(TotalAmount, PatientId, PaymentMethod);