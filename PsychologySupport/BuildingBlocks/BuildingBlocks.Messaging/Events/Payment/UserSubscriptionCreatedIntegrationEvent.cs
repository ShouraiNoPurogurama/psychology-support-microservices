namespace Subscription.API.Events
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
