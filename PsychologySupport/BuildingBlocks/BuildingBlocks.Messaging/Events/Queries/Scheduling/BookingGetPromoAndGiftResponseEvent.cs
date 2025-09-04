namespace BuildingBlocks.Messaging.Events.Queries.Scheduling
{
    public record BookingGetPromoAndGiftResponseEvent(
         string? PromoCode,
         Guid? GiftId
     );
}
