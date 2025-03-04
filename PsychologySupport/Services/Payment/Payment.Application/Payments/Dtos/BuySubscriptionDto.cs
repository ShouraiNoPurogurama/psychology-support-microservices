using BuildingBlocks.Enums;

namespace Payment.Application.Payments.Dtos;

public record BuySubscriptionDto(
    Guid ServicePackageId,
    Guid? PromotionCodeId,
    Guid? GiftId,
    decimal TotalAmount,
    Guid PatientId,
    int DurationDays,
    PaymentMethodName PaymentMethod)
    : BasePaymentDto(TotalAmount, PatientId, PaymentMethod);