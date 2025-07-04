using Auth.API.Dtos.Requests;
using Auth.API.Dtos.Responses;

namespace Auth.API.ServiceContracts;

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
}