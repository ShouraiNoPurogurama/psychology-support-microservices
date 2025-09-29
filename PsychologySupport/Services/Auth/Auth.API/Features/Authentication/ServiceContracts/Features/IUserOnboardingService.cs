using Auth.API.Features.Authentication.Dtos.Responses;

namespace Auth.API.Features.Authentication.ServiceContracts.Features;

public interface IUserOnboardingService
{
    Task<bool> MarkPiiOnboardedAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> MarkPatientOnboardedAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserOnboardingStatusResponse> GetOnboardingStatusAsync(CancellationToken cancellationToken = default);
}