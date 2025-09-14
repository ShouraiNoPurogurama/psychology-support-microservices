using Auth.API.Domains.Authentication.Dtos.Responses;

namespace Auth.API.Domains.Authentication.ServiceContracts.Features;

public interface IUserOnboardingService
{
    Task<bool> MarkPiiOnboardedAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> MarkPatientOnboardedAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserOnboardingStatusDto> GetOnboardingStatusAsync(Guid userId, CancellationToken cancellationToken = default);
}