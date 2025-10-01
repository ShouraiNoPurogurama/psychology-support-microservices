using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YarpApiGateway.Features.TokenExchange.Contracts;
using YarpApiGateway.Features.TokenExchange.Rules;

namespace YarpApiGateway.Features.TokenExchange;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly IInternalTokenMintingService _tokenMintingService;
    private readonly TokenExchangeRuleRegistry _ruleRegistry;
    private readonly ILogger<TokenExchangeService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenExchangeService(TokenExchangeRuleRegistry ruleRegistry, IInternalTokenMintingService tokenMintingService, ILogger<TokenExchangeService> logger)
    {
        _ruleRegistry = ruleRegistry;
        _tokenMintingService = tokenMintingService;
        _logger = logger;
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
                
                _logger.LogInformation($"Found audience for {rule.Keywords.First()}");
                
                //Nếu ID bị empty tức là user chưa tạo alias hoặc chưa làm onboarding => trả null cho caller 
                //viiết 403 vào response
                if (string.IsNullOrEmpty(newId) || Guid.Parse(newId) == Guid.Empty)
                {
                    _logger.LogWarning("*** Claims fetching failed for: `{rule}` with result got {newId}", rule.ClaimType, newId);

                    return null;
                }

                var newClaims = new List<Claim> { new Claim(rule.ClaimType, newId) };

                var exchangedToken = _tokenMintingService.MintScopedToken(
                    new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims)),
                    newClaims, destinationAudience
                );
                
                _logger.LogInformation("Exchanged token minted for audience {Audience}: {Token}", destinationAudience, exchangedToken);
                return exchangedToken;
            }
        }

        //Không tìm thấy quy tắc nào khớp
        return originalToken;
    }
}