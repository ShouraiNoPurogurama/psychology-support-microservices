using System.Security.Claims;
using BuildingBlocks.Exceptions;

namespace Alias.API.Common.Authentication;

public sealed class CurrentActorAccessor : ICurrentActorAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<CurrentActorAccessor> _logger;

    public CurrentActorAccessor(IHttpContextAccessor http, ILogger<CurrentActorAccessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool TryGetAliasId(out Guid aliasId)
    {
        aliasId = Guid.Empty;

        var aliasIdStr = _http.HttpContext?.User.FindFirstValue("aliasId");

        if (!Guid.TryParse(aliasIdStr, out var id) || id == Guid.Empty)
        {
            _logger.LogWarning("Missing or invalid aliasId claim.");
            return false;
        }

        aliasId = id;
        return true;
    }

    public Guid GetRequiredAliasId()
    {
        if (TryGetAliasId(out var id)) return id;
        throw new UnauthorizedException("Yêu cầu không hợp lệ.", "CLAIMS_MISSING");
    }

    public bool TryGetSubjectRef(out Guid subjectRef)
    {
        subjectRef = Guid.Empty;

        var subjectRefStr = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subjectRefStr, out var id) || id == Guid.Empty)
        {
            _logger.LogWarning("Missing or invalid subjectRef claim.");
            return false;
        }

        subjectRef = id;
        return true;
    }

    public Guid GetRequiredSubjectRef()
    {
        if (TryGetSubjectRef(out var id)) return id;
        throw new UnauthorizedException("Yêu cầu không hợp lệ.", "CLAIMS_MISSING");
    }
}