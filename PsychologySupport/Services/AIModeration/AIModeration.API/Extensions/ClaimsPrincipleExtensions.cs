using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AIModeration.API.Extensions;

public static class ClaimsPrincipalExtensions
{

    public static string GetAliasId(this ClaimsPrincipal user)
    {
        var result = user.Claims.FirstOrDefault(c => c.Type == "aliasId")?.Value ??
                     throw new UnauthorizedAccessException("Token không hợp lệ: Không tìm thấy Claims Alias Id.");
        
        return result;
    }
    

    public static ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null) return null;

        var identity = new ClaimsIdentity(jwtToken.Claims);
        return new ClaimsPrincipal(identity);
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        var roleClaims = user
            .FindFirst(ClaimTypes.Role);

        return roleClaims.Value;
    }
}