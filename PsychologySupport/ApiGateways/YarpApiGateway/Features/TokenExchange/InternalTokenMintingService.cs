using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class InternalTokenMintingService : IInternalTokenMintingService
{
    private readonly IConfiguration _configuration;

    public InternalTokenMintingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string MintScopedToken(ClaimsPrincipal originalPrincipal, IEnumerable<Claim> additionalClaims, string audience)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:InternalKey"]!));
        var signingCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = originalPrincipal.Claims
            .Where(c => c.Type != JwtRegisteredClaimNames.Aud) 
            .ToList();

        claims.AddRange(additionalClaims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredential
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}