namespace Payment.Application.Dtos;

public record BuySubscriptionDto(Guid ServicePackageId, Guid? PromotionCodeId, Guid? GiftId, decimal TotalAmount, Guid PatientId, PaymentMethod PaymentMethod)
    : BasePaymentDto(TotalAmount, PatientId, PaymentMethod);