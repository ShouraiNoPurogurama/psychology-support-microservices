using System.Security.Claims;

namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface ITokenService
{
    // ReSharper disable once InconsistentNaming
    (string Token, string Jti)  GenerateJWTFromOldToken(string oldToken, string? subscriptionPlanNameOverride = null);
    
    Task<(string Token, string Jti)> GenerateJWTToken(User user);
    
    bool VerifyPassword(string providedPassword, string hashedPassword, User user);

    string HashPassword(User user, string password);

    string GenerateRefreshToken();
    
    (string newJwtToken, string jti, string newRefreshToken) RefreshToken(string oldJwt, string? subscriptionPlanNameOverride = null);

    Task<(string AccessToken, string Jti, string RefreshToken)> GenerateTokensAsync(User user);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}