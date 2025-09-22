using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;
using Auth.API.Features.Authentication.ServiceContracts;
using Auth.API.Features.Authentication.ServiceContracts.Features;

namespace Auth.API.Features.Authentication.Services;

public class AuthFacade(
    IUserRegistrationService registrationService,
    IPasswordService passwordService,
    ISessionService sessionService,
    IUserAccountService userAccountService
)
    : IAuthFacade
{
    public Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        return registrationService.RegisterAsync(registerRequest);
    }

    public Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest)
    {
        return registrationService.ConfirmEmailAsync(confirmEmailRequest);
    }

    public Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        return sessionService.LoginAsync(loginRequest);
    }

    public Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        return sessionService.GoogleLoginAsync(request);
    }

    public Task<bool> UnlockAccountAsync(string email)
    {
        return userAccountService.UnlockAccountAsync(email);
    }

    public Task<LoginResponse> RefreshAsync(TokenApiRequest tokenApiRequest)
    {
        return sessionService.RefreshAsync(tokenApiRequest);
    }

    public Task<bool> RevokeAsync(TokenApiRequest tokenApiRequest)
    {
        return sessionService.RevokeAsync(tokenApiRequest);
    }

    public Task<bool> ForgotPasswordAsync(string email)
    {
        return passwordService.ForgotPasswordAsync(email);
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return passwordService.ResetPasswordAsync(request);
    }

    public Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        return passwordService.ChangePasswordAsync(request);
    }
}