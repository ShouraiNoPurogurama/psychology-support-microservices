using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Profile.API.Domains.Public.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetPatientId(this ClaimsPrincipal user)
    {
        var result = user.Claims.FirstOrDefault(c => c.Type == "patientId")?.Value ??
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