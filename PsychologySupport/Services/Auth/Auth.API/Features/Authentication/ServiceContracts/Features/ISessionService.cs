using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;

namespace Auth.API.Features.Authentication.ServiceContracts.Features;

public interface ISessionService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<LoginResponse> RefreshAsync(TokenApiRequest request);
    Task<bool> RevokeAsync(TokenApiRequest request);
}