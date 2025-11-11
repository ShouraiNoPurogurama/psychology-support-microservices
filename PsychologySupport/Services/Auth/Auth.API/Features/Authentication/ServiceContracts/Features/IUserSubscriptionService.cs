namespace Auth.API.Features.Authentication.ServiceContracts.Features
{
    public interface IUserSubscriptionService
    {
        Task<bool> UpdateSubscriptionPlanNameAsync(
            Guid subjectRef,
            string subscriptionPlanName,
            DateTimeOffset validFrom,
            DateTimeOffset validTo
        );
        Task<bool> RemoveExpiredSubscriptionAsync(Guid patientId);
        Task<bool> ActivateFreeTrialAsync(Guid subjectRef);
    }
}
