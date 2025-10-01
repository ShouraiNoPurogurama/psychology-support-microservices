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

        _logger.LogInformation("[InternalTokenMintingService] Minting new token for audience {Audience} with issuer {Issuer}", audience, issuer);
        
        var signingCredential = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        //copy claims gốc trừ Aud → merge thêm claims mới
        var claims = originalPrincipal.Claims
            .Where(c => c.Type != JwtRegisteredClaimNames.Aud)
            .ToList();

        claims.AddRange(additionalClaims);

        _logger.LogInformation("Original claims: {OriginalClaims}", string.Join(", ", originalPrincipal.Claims.Select(c => $"{c.Type}:{c.Value}")));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTimeOffset.UtcNow.UtcDateTime.AddMinutes(15),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredential
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        _logger.LogInformation("*** New minted token : {Token}", token);
        
        var result = tokenHandler.WriteToken(token);

        _logger.LogInformation("******* Minted new token: {Token}", result);

        return result;
    }

    private RsaSecurityKey GetRsaSecurityKey()
    {
        var rsa = RSA.Create();
        string xmlKey = File.ReadAllText(_configuration["Jwt:PrivateKeyPath"]!);
        rsa.FromXmlString(xmlKey);
        return new RsaSecurityKey(rsa);
    }
}