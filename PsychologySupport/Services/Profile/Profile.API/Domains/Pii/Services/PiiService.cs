using Grpc.Core;
using Pii.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Features.SeedPersonProfile;
using PersonSeedDto = Profile.API.Domains.Pii.Dtos.PersonSeedDto;
using UserGender = BuildingBlocks.Enums.UserGender;

namespace Profile.API.Domains.Pii.Services;

public class PiiService(PiiDbContext piiDbContext, ISender sender, ILogger<PiiService> logger)
    : global::Pii.API.Protos.PiiService.PiiServiceBase
{
    public override Task<ResolvePersonInfoBySubjectRefResponse> ResolvePersonInfoBySubjectRef(
        ResolvePersonInfoBySubjectRefRequest request, ServerCallContext context)
    {
        return base.ResolvePersonInfoBySubjectRef(request, context);
    }

    public override async Task<ResolvePatientIdBySubjectRefResponse> ResolvePatientIdBySubjectRef(
        ResolvePatientIdBySubjectRefRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SubjectRef, out var subjectRef))
            return new ResolvePatientIdBySubjectRefResponse
            {
                PatientId = null
            };

        var patientProfileId = await piiDbContext.PatientOwnerMaps.AsNoTracking()
            .Where(a => a.SubjectRef == subjectRef)
            .Select(a => a.PatientProfileId)
            .FirstOrDefaultAsync();

        return new ResolvePatientIdBySubjectRefResponse
        {
            PatientId = patientProfileId.ToString()
        };
    }

    public override async Task<ResolveAliasIdBySubjectRefResponse> ResolveAliasIdBySubjectRef(
        ResolveAliasIdBySubjectRefRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SubjectRef, out var subjectRef))
            return new ResolveAliasIdBySubjectRefResponse
            {
                AliasId = null
            };

        var aliasId = await piiDbContext.AliasOwnerMaps.AsNoTracking()
            .Where(a => a.SubjectRef == subjectRef)
            .Select(a => a.AliasId)
            .FirstOrDefaultAsync();

        return new ResolveAliasIdBySubjectRefResponse
        {
            AliasId = aliasId.ToString()
        };
    }


    public override async Task<ResolveSubjectRefByAliasIdResponse> ResolveSubjectRefByAliasId(
        ResolveSubjectRefByAliasIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
            return new ResolveSubjectRefByAliasIdResponse
            {
                SubjectRef = Guid.Empty.ToString()
            };

        var subjectRef = await piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .Where(x => x.AliasId == aliasId)
            .Select(x => x.SubjectRef)
            .FirstOrDefaultAsync(context.CancellationToken);

        return new ResolveSubjectRefByAliasIdResponse
        {
            SubjectRef = subjectRef.ToString()
        };
    }


    public override async Task<ResolveSubjectRefByUserIdResponse> ResolveSubjectRefByUserId(
        ResolveSubjectRefByUserIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new ResolveSubjectRefByUserIdResponse
            {
                SubjectRef = Guid.Empty.ToString()
            };

        var subjectRef = await piiDbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.SubjectRef)
            .FirstOrDefaultAsync();

        if (subjectRef == Guid.Empty)
            throw new RpcException(new Status(StatusCode.NotFound, "Profile not found"));

        return new ResolveSubjectRefByUserIdResponse
        {
            SubjectRef = subjectRef.ToString()
        };
    }

    public override async Task<ResolveUserIdBySubjectRefResponse> ResolveUserIdBySubjectRef(
        ResolveUserIdBySubjectRefRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.SubjectRef, out var subjectRef))
            return new ResolveUserIdBySubjectRefResponse
            {
                UserId = Guid.Empty.ToString()
            };

        var userId = await piiDbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.SubjectRef == subjectRef)
            .Select(p => p.UserId)
            .FirstOrDefaultAsync();

        return new ResolveUserIdBySubjectRefResponse
        {
            UserId = userId.ToString()
        };
    }

    public async Task<Guid> ResolveUserIdByPatientId(Guid patientId, CancellationToken cancellationToken)
    {
        var userId = await
            (
                from pom in piiDbContext.PatientOwnerMaps.AsNoTracking()
                join pp in piiDbContext.PersonProfiles.AsNoTracking()
                    on pom.SubjectRef equals pp.SubjectRef
                where pom.PatientProfileId == patientId
                select pp.UserId
            )
            .FirstOrDefaultAsync(cancellationToken
            );

        return userId;
    }
    
    public override async Task<ResolveUserIdByAliasIdResponse> ResolveUserIdByAliasId(
        ResolveUserIdByAliasIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
            return new ResolveUserIdByAliasIdResponse 
            { 
                UserId = Guid.Empty.ToString() 
            };

        var query = from am in piiDbContext.AliasOwnerMaps.AsNoTracking()
            join pp in piiDbContext.PersonProfiles.AsNoTracking()
                on am.SubjectRef equals pp.SubjectRef
            where am.AliasId == aliasId
            select pp.UserId;
        
        var userId = await query.FirstOrDefaultAsync(context.CancellationToken);

        return new ResolveUserIdByAliasIdResponse
        {
            UserId = userId.ToString()
        };
    }

    public override Task<ResolvePatientIdByAliasIdResponse> ResolvePatientIdByAliasId(ResolvePatientIdByAliasIdRequest request,
        ServerCallContext context)
    {
        return base.ResolvePatientIdByAliasId(request, context);
    }

    public override async Task<ResolvePersonInfoByPatientIdResponse> ResolvePersonInfoByPatientId(
    ResolvePersonInfoByPatientIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.PatientId, out var patientId))
        {
            return new ResolvePersonInfoByPatientIdResponse
            {
                SubjectRef = Guid.Empty.ToString(),
                Email = string.Empty
            };
        }

        // Join PatientOwnerMap -> PersonProfile
        var result = await (
            from pom in piiDbContext.PatientOwnerMaps.AsNoTracking()
            join pp in piiDbContext.PersonProfiles.AsNoTracking()
                on pom.SubjectRef equals pp.SubjectRef
            where pom.PatientProfileId == patientId
            select new
            {
                pp.SubjectRef,
                pp.ContactInfo.Email
            }
        ).FirstOrDefaultAsync(context.CancellationToken);

        if (result is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "No profile found for this patientId"));
        }

        return new ResolvePersonInfoByPatientIdResponse
        {
            SubjectRef = result.SubjectRef.ToString(),
            Email = result.Email ?? string.Empty
        };
    }

}