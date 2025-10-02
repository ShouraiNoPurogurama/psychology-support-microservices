namespace Auth.API.Features.Authentication.ServiceContracts.Features
{
    public interface IUserSubscriptionService
    {
 
        Task<bool> UpdateSubscriptionPlanNameAsync(Guid SubjectRef, string subscriptionPlanName);
    }
}
