using Auth.API.Domains.Authentication.Dtos.Responses;

namespace Auth.API.Domains.Authentication.ServiceContracts.Features;

public interface ISessionService
{
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<LoginResponse> RefreshAsync(TokenApiRequest request);
    Task<bool> RevokeAsync(TokenApiRequest request);
}