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

    public TokenExchangeService(TokenExchangeRuleRegistry ruleRegistry, IInternalTokenMintingService tokenMintingService,
        ILogger<TokenExchangeService> logger)
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

        var rules = _ruleRegistry.GetRules(subjectRef).ToList();

        // Gom tất cả rule khớp audience
        var matched = rules
            .Where(r => r.Keywords.Any(k => destinationAudience.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matched.Count == 0)
        {
            _logger.LogInformation("No matching rule found for audience {Audience}. Returning original token.",
                destinationAudience);
            return originalToken;
        }

        // (optional) timeout chung cho tất cả lookup
        var timeout = TimeSpan.FromSeconds(10);

        // Tạo tasks chạy song song, mỗi task tự bắt lỗi và trả về kết quả an toàn
        var tasks = matched.Select(async r =>
            {
                try
                {
                    // Nếu bạn muốn timeout per-call mà LookupFunction không nhận token,
                    // dùng helper WithTimeout (ở dưới) để wrap:
                    var id = await WithTimeout(r.LookupFunction(subjectRef), timeout);
                    return (rule: r, id, ok: true, ex: (Exception?)null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lookup failed for claim {ClaimType}", r.ClaimType);
                    return (rule: r, id: null, ok: false, ex);
                }
            })
            .ToList();

        // Chờ tất cả xong
        var results = await Task.WhenAll(tasks);

        // Đánh giá kết quả
        var newClaims = new List<Claim>();

        foreach (var res in results)
        {
            var r = res.rule;
            var newId = res.id;

            _logger.LogInformation("Audience {Audience}: rule {ClaimType} lookup → {Value}",
                destinationAudience, r.ClaimType, newId ?? "null");

            var valid = !string.IsNullOrEmpty(newId) && Guid.TryParse(newId, out var g) && g != Guid.Empty;

            if (!valid)
            {
                if (r.Required)
                {
                    _logger.LogWarning("Required claim `{ClaimType}` missing/invalid for audience {Audience}.",
                        r.ClaimType, destinationAudience);
                    return null; // 403 ở transform 
                }

                continue; // optional claim: bỏ qua
            }

            newClaims.Add(new Claim(r.ClaimType, newId!));
        }

        // Tránh trùng kiểu claim (nếu có nhiều rule cùng ClaimType)
        newClaims = newClaims
            .GroupBy(c => c.Type, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        if (newClaims.Count == 0)
        {
            _logger.LogWarning("No claims produced for audience {Audience}.", destinationAudience);
            return null;
        }

        var exchangedToken = _tokenMintingService.MintScopedToken(
            new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims)),
            newClaims,
            destinationAudience
        );

        return exchangedToken;

        // Helper: timeout cho Task<T> khi không truyền được CancellationToken vào LookupFunction
        static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
        {
            var delay = Task.Delay(timeout);
            var finished = await Task.WhenAny(task, delay);
            if (finished == delay)
                throw new TimeoutException($"Lookup timed out after {timeout.TotalMilliseconds} ms.");
            return await task; 
        }
    }
}