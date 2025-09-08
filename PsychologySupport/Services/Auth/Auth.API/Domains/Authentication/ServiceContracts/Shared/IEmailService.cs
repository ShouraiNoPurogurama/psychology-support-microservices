namespace Auth.API.Domains.Authentication.ServiceContracts.Shared;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(User user);
    Task SendPasswordResetEmailAsync(User user);
}