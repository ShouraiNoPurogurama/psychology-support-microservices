using Google.Apis.Auth;

namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface IUserProvisioningService
{
    Task<User> FindOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload);
}