using Auth.API.Domains.Authentication.Dtos.Responses;

namespace Auth.API.Domains.Authentication.ServiceContracts.v1;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest registerRequest);
    Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest);
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<bool> UnlockAccountAsync(string email);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<LoginResponse> RefreshAsync(TokenApiRequest tokenApiRequest);
    Task<bool> RevokeAsync(TokenApiRequest tokenApiRequest);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}