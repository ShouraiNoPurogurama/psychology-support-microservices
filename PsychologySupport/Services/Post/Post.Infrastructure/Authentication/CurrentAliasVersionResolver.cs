using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Post.Application.Abstractions.Authentication;
using Post.Infrastructure.Data.Query;

namespace Post.Infrastructure.Authentication;

internal sealed class CurrentAliasVersionResolver : IActorResolver, IAliasVersionResolver
{
    private readonly QueryDbContext _dbContext;
    private readonly ILogger<CurrentAliasVersionResolver> _logger;

    public Guid AliasId { get; }

    public CurrentAliasVersionResolver(
        IHttpContextAccessor httpContextAccessor,
        QueryDbContext dbContext,
        ILogger<CurrentAliasVersionResolver> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

        if (!Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue("aliasId"), out var aliasId))
        {
            throw new UnauthorizedAccessException("Đã xảy ra lỗi khi xử lý phiên đăng nhập.");
        }

        AliasId = aliasId;
    }

    public async Task<Guid> GetCurrentAliasVersionIdAsync(CancellationToken cancellationToken = default)
    {
        var replicaVersion = await _dbContext.AliasVersionReplica
            .Where(a => a.AliasId == AliasId)
            .Select(a => (Guid?)a.CurrentVersionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (replicaVersion.HasValue && replicaVersion.Value != Guid.Empty)
        {
            return replicaVersion.Value;
        }

        _logger.LogError("Data replication failure: Could not find replicated alias version for AliasId: {AliasId}",
            this.AliasId);
        throw new InvalidOperationException("Đã xảy ra lỗi khi xử lý phiên đăng nhập.");
    }
}