namespace Subscription.API.UserSubscriptions.Dtos
{
    public record GetSubscriptionPricingDto(
        Guid PatientId,
        Guid ServicePackageId,
        string? PromoCode,
        Guid? GiftId
    );
}
