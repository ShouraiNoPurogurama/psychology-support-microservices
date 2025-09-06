using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Alias.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAliasId(this ClaimsPrincipal user)
    {
        var result = user.Claims.FirstOrDefault(c => c.Type == "aliasId")?.Value ??
                     throw new UnauthorizedAccessException("Token không hợp lệ.");
        
        return Guid.Parse(result);
    }
    
    public static Guid GetSubjectRef(this ClaimsPrincipal user)
    {
        var result = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                     throw new UnauthorizedAccessException("Token không hợp lệ.");
        
        return Guid.Parse(result);
    }
    
    public static IEnumerable<Claim> GetAllClaims(this ClaimsPrincipal user)
    {
        return user.Claims;
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