using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;

namespace Auth.API.Features.Authentication.ServiceContracts;

public interface IAuthFacade
{
    Task<bool> RegisterAsync(RegisterRequest registerRequest);
    Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest);

    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<LoginResponse> RefreshAsync(TokenApiRequest tokenApiRequest);
    Task<bool> RevokeAsync(TokenApiRequest tokenApiRequest);

    Task<bool> UnlockAccountAsync(string email);

    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}