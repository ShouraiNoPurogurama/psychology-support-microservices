using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange;

public class InternalTokenMintingService : IInternalTokenMintingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<InternalTokenMintingService> _logger;

    public InternalTokenMintingService(
        IConfiguration configuration,
        ILogger<InternalTokenMintingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string MintScopedToken(
        ClaimsPrincipal originalPrincipal,
        IEnumerable<Claim> additionalClaims,
        string audience)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var rsaKey = GetRsaSecurityKey();

        var signingCredential = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        //copy claims gốc trừ Aud → merge thêm claims mới
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

    private RsaSecurityKey GetRsaSecurityKey()
    {
        var rsa = RSA.Create();
        string xmlKey = File.ReadAllText(_configuration["Jwt:PrivateKeyPath"]!);
        rsa.FromXmlString(xmlKey);
        return new RsaSecurityKey(rsa);
    }
}