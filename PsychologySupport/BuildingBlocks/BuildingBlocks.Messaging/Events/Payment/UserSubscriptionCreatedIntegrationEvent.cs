namespace BuildingBlocks.Messaging.Events.Payment
{
    public record UserSubscriptionCreatedIntegrationEvent(
       Guid SubscriptionId,
       Guid PatientId,
       Guid ServicePackageId,
       decimal Price,
       Guid? PromoCodeId,
       Guid? GiftId,
       DateTime StartDate,
       DateTime EndDate
   );
}
