using System.Security.Claims;
using Auth.API.Models;

namespace Auth.API.ServiceContracts;

public interface ITokenService
{
    // ReSharper disable once InconsistentNaming
    Task<(string Token, string Jti)> GenerateJWTToken(User user);

    Guid GetUserIdFromHttpContext(HttpContext httpContext);

    bool VerifyPassword(string providedPassword, string hashedPassword, User user);

    string HashPassword(User user, string password);

    string GenerateRefreshToken();

    Task SaveRefreshToken(User user, string refreshToken);

    Task<bool> ValidateRefreshToken(User user, string refreshToken);

    Task<(string newJwtToken, string newRefreshToken)> RefreshToken(User user, string refreshToken);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}