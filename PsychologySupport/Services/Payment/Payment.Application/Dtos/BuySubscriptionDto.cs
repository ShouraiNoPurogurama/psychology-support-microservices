namespace Payment.Application.Dtos;

public record BuySubscriptionDto(Guid ServicePackageId, Guid PromotionCodeId, Guid GiftId)
    : BasePaymentDto(default, default, default);