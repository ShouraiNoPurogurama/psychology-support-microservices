using Auth.API.Features.Authentication.Dtos.Requests;

namespace Auth.API.Features.Authentication.ServiceContracts.Features;

public interface IUserRegistrationService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string> ConfirmEmailAsync(ConfirmEmailRequest request);
}