using Grpc.Core;
using Pii.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;

namespace Profile.API.Domains.Pii.Services;

public class PiiService : global::Pii.API.Protos.PiiService.PiiServiceBase
{
    private readonly PiiDbContext _piiDbContext;
    private readonly ProfileDbContext _profileDbContext;

    public PiiService(PiiDbContext piiDbContext, ProfileDbContext profileDbContext)
    {
        _piiDbContext = piiDbContext;
        _profileDbContext = profileDbContext;
    }

    public override Task<ResolvePersonInfoBySubjectRefResponse> ResolvePersonInfoBySubjectRef(
        ResolvePersonInfoBySubjectRefRequest request, ServerCallContext context)
    {
        return base.ResolvePersonInfoBySubjectRef(request, context);
    }

    public override async Task<ResolveSubjectRefByAliasIdResponse> ResolveSubjectRefByAliasId(
        ResolveSubjectRefByAliasIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
            return new ResolveSubjectRefByAliasIdResponse
            {
                SubjectRef = null
            };

        var subjectRef = await _piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .Where(x => x.AliasId == aliasId)
            .Select(x => x.SubjectRef)
            .FirstOrDefaultAsync(context.CancellationToken);

        return new ResolveSubjectRefByAliasIdResponse
        {
            SubjectRef = subjectRef.ToString()
        };
    }

   
    public override async Task<ResolveSubjectRefByUserIdResponse> ResolveSubjectRefByUserId(ResolveSubjectRefByUserIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new ResolveSubjectRefByUserIdResponse
            {
                SubjectRef = null
            };

        var subjectRef = await _piiDbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.SubjectRef)
            .FirstOrDefaultAsync();

        return new ResolveSubjectRefByUserIdResponse
        {
            SubjectRef = subjectRef.ToString()
        };
    }

    public override async Task<CreateSubjectRefResponse> CreateSubjectRef(CreateSubjectRefRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new CreateSubjectRefResponse
            {
                SubjectRef = null
            };

        var newSubjectRef = Guid.NewGuid();
        
        return new CreateSubjectRefResponse
        {
            SubjectRef = newSubjectRef.ToString()
        };
    }

    public override Task<ResolveUserIdBySubjectRefResponse> ResolveUserIdBySubjectRef(ResolveUserIdBySubjectRefRequest request,
        ServerCallContext context)
    {
        return base.ResolveUserIdBySubjectRef(request, context);
    }

    public override Task<ResolvePatientIdByAliasIdResponse> ResolvePatientIdByAliasId(ResolvePatientIdByAliasIdRequest request,
        ServerCallContext context)
    {
        return base.ResolvePatientIdByAliasId(request, context);
    }
}