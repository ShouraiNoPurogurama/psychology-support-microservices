namespace Auth.API.Domains.Authentication.ServiceContracts.Features;

public interface IUserRegistrationService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string> ConfirmEmailAsync(ConfirmEmailRequest request);
}