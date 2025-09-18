using System.Security.Claims;

namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface ITokenService
{
    // ReSharper disable once InconsistentNaming
    Task<(string Token, string Jti)> GenerateJWTToken(User user);

    Guid GetAliasIdFromHttpContext(HttpContext httpContext);

    bool VerifyPassword(string providedPassword, string hashedPassword, User user);

    string HashPassword(User user, string password);

    string GenerateRefreshToken();

    Task SaveRefreshToken(User user, string refreshToken);

    Task<bool> ValidateRefreshToken(User user, string refreshToken);

    Task<(string newJwtToken, string newRefreshToken)> RefreshToken(User user, string refreshToken);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}