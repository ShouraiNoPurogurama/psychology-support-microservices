using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange.Decorators;

public class CachedInternalTokenMintingService : IInternalTokenMintingService
{
    private readonly IInternalTokenMintingService _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedInternalTokenMintingService> _logger;

    public CachedInternalTokenMintingService(
        IInternalTokenMintingService inner,
        IMemoryCache cache,
        ILogger<CachedInternalTokenMintingService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public string MintScopedToken(
        ClaimsPrincipal originalPrincipal,
        IEnumerable<Claim> additionalClaims,
        string audience)
    {
        var cacheKey = BuildCacheKey(originalPrincipal, additionalClaims, audience);

        if (_cache.TryGetValue(cacheKey, out string? token))
        {
            _logger.LogInformation("[Cache HIT] for key {CacheKey}", cacheKey);
            return token!;
        }

        token = _inner.MintScopedToken(originalPrincipal, additionalClaims, audience);

        //TTL = 15 phút
        _cache.Set(cacheKey, token, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
            Size = 1 //coi mỗi token = 1 đơn vị trong 200k đơn vị bộ nhớ
        });

        _logger.LogDebug("Cache MISS for key {CacheKey}, minted new token", cacheKey);

        return token;
    }

    private static string BuildCacheKey(
        ClaimsPrincipal principal,
        IEnumerable<Claim> additionalClaims,
        string audience)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "";
        var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "";

        var extra = string.Join("|", additionalClaims.Select(c => $"{c.Type}:{c.Value}"));

        var rawKey = $"{sub}|{role}|{extra}|{audience}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(hash);
    }
}
