using System.Security.Claims;
using BuildingBlocks.Exceptions;

namespace Auth.API.Common.Authentication;

public sealed class CurrentActorAccessor : ICurrentActorAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<CurrentActorAccessor> _logger;

    public CurrentActorAccessor(IHttpContextAccessor http, ILogger<CurrentActorAccessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool TryGetSubjectRef(out Guid subjectRef)
    {
        subjectRef = Guid.Empty;
        
        var subjectRefStr =  _http.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;;
        
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