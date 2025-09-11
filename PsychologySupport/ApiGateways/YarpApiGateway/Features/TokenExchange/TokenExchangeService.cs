using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly IPiiLookupService _piiLookupService;
    private readonly IInternalTokenMintingService _tokenMintingService;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenExchangeService(IPiiLookupService piiLookupService, IInternalTokenMintingService tokenMintingService)
    {
        _piiLookupService = piiLookupService;
        _tokenMintingService = tokenMintingService;
    }

    public async Task<string?> ExchangeTokenAsync(string originalToken, string destinationAudience)
    {
        var jwtToken = _tokenHandler.ReadJwtToken(originalToken);

        var subjectRef = jwtToken.Subject;

        if (string.IsNullOrEmpty(subjectRef)) return null;

        // Đây là nơi quyết định xem cần tra cứu ID nào dựa vào audience
        // Ví dụ: nếu destinationAudience là "social-service", ta tra cứu aliasId
        List<Claim> newClaims = new();

        if (destinationAudience.Contains("alias") || destinationAudience.Contains("post") || destinationAudience.Contains("feed"))
        {
            var aliasId = await _piiLookupService.ResolveAliasIdBySubjectRefAsync(subjectRef);
            if (string.IsNullOrEmpty(aliasId)) return null;
            newClaims.Add(new Claim("aliasId", aliasId));
        }
        else if (destinationAudience.Contains("profile"))
        {
            var patientId = await _piiLookupService.ResolvePatientIdBySubjectRefAsync(subjectRef);
            if (string.IsNullOrEmpty(patientId)) return null;
            newClaims.Add(new Claim("patientId", patientId));
        }
        else
        {
            //Không có quy tắc nào khớp, không exchange
            return originalToken;
        }

        return _tokenMintingService.MintScopedToken
        (
            new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims)),
            newClaims, destinationAudience
        );
    }
}