namespace Subscription.API.Dtos
{
    public record GetUserSubscriptionDto
    (
        Guid Id,
        Guid PatientId,  
        Guid ServicePackageId,
        DateTime StartDate,
        DateTime EndDate,
        string Status
    );
}
