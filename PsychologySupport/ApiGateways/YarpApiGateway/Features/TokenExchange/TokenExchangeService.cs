using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YarpApiGateway.Features.TokenExchange.Contracts;
using YarpApiGateway.Features.TokenExchange.Rules;

namespace YarpApiGateway.Features.TokenExchange;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly IInternalTokenMintingService _tokenMintingService;
    private readonly TokenExchangeRuleRegistry _ruleRegistry;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenExchangeService(TokenExchangeRuleRegistry ruleRegistry, IInternalTokenMintingService tokenMintingService)
    {
        _ruleRegistry = ruleRegistry;
        _tokenMintingService = tokenMintingService;
    }

    public async Task<string?> ExchangeTokenAsync(string originalToken, string destinationAudience)
    {
        var jwtToken = _tokenHandler.ReadJwtToken(originalToken);
        var subjectRef = jwtToken.Subject;

        if (string.IsNullOrEmpty(subjectRef)) return null;

        var rules = _ruleRegistry.GetRules(subjectRef);
        foreach (var rule in rules)
        {
            //Kiểm tra xem destinationAudience có chứa bất kỳ từ khóa nào trong Keywords không
            if (rule.Keywords.Any(destinationAudience.Contains))
            {
                var newId = await rule.LookupFunction(subjectRef);
                if (string.IsNullOrEmpty(newId)) return null;

                var newClaims = new List<Claim> { new Claim(rule.ClaimType, newId) };

                return _tokenMintingService.MintScopedToken(
                    new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims)),
                    newClaims, destinationAudience
                );
            }
        }

        //Không tìm thấy quy tắc nào khớp
        return originalToken;
    }
}