namespace Subscription.API.Events
{
    public record UserSubscriptionCreatedEvent(
       Guid SubscriptionId,
       Guid PatientId,
       Guid ServicePackageId,
       DateTime StartDate,
       DateTime EndDate
   );
}
