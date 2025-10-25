using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.ReadModels.Models;

namespace Post.Infrastructure.Authentication;

internal sealed class AliasVersionAccessor : IAliasVersionAccessor
{
    private static readonly Func<DbContext, Guid, Task<Guid?>>
        SGetCurrentAliasVersionAsync =
            EF.CompileAsyncQuery((DbContext db, Guid aliasId) =>
                db.Set<AliasVersionReplica>()
                    .AsNoTracking()
                    .Where(a => a.AliasId == aliasId)
                    .OrderByDescending(a => a.LastSyncedAt)
                    .Select(a => (Guid?)a.CurrentVersionId)
                    .FirstOrDefault());

    private readonly ICurrentActorAccessor _actor;
    private readonly IQueryDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AliasVersionAccessor> _logger;

    public AliasVersionAccessor(
        ICurrentActorAccessor actor,
        IQueryDbContext db,
        IMemoryCache cache,
        ILogger<AliasVersionAccessor> logger)
    {
        _actor = actor;
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<(bool ok, Guid aliasVersionId)> TryGetCurrentAliasVersionIdAsync(CancellationToken ct = default)
    {
        if (!_actor.TryGetAliasId(out var aliasId))
            return (false, Guid.Empty);

        var cacheKey = $"alias-version:{aliasId}";
        if (_cache.TryGetValue(cacheKey, out Guid cached))
            return (true, cached);

        Guid? version = null;
        if (_db is DbContext ef)
        {
            version = await SGetCurrentAliasVersionAsync(ef, aliasId);
        }

        if (version.HasValue && version.Value != Guid.Empty)
        {
            _cache.Set(cacheKey, version.Value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(45),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return (true, version.Value);
        }

        _logger.LogError("Replication missing for AliasId {AliasId}.", aliasId);
        return (false, Guid.Empty);
    }

    public async Task<Guid> GetRequiredCurrentAliasVersionIdAsync(CancellationToken ct = default)
    {
        var (ok, id) = await TryGetCurrentAliasVersionIdAsync(ct);
        if (ok) return id;
        if (!_actor.TryGetAliasId(out _))
            throw new UnauthorizedException("Yêu cầu không hợp lệ.", "CLAIMS_MISSING");
        throw new InvalidOperationException("Không tìm thấy phiên bản hồ sơ bí danh cho phiên hiện tại.");
    }
}