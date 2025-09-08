namespace Auth.API.Domains.Authentication.ServiceContracts.Features.v4;

public interface IUserRegistrationService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string> ConfirmEmailAsync(ConfirmEmailRequest request);
}