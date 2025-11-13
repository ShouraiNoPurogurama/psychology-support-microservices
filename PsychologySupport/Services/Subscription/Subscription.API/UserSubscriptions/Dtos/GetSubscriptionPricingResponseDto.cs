namespace Subscription.API.UserSubscriptions.Dtos
{
    public record GetSubscriptionPricingResponseDto(
        decimal OriginalPrice,
        decimal DiscountAmount,
        decimal FinalPrice,
        string Status
    );
}
