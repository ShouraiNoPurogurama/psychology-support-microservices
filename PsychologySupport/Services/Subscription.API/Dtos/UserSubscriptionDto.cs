using Subscription.API.Data.Common;

namespace Subscription.API.Dtos
{
    public record UserSubscriptionDto(
        Guid Id,
        Guid PatientId,  
        Guid ServicePackageId,
        DateTime StartDate,
        DateTime EndDate,
        SubscriptionStatus Status 
    );
}
