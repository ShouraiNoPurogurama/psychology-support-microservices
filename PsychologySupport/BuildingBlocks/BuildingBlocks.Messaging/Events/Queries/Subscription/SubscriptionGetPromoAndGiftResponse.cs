namespace BuildingBlocks.Messaging.Events.Queries.Subscription
{
    public record SubscriptionGetPromoAndGiftResponse(
        string? PromoCode,
        Guid ? GiftId
    );
}
