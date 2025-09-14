using Grpc.Core;
using Pii.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Features.EnsureSubjectRef;
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
                SubjectRef = null
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
                SubjectRef = null
            };

        var subjectRef = await piiDbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.SubjectRef)
            .FirstOrDefaultAsync();

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

    public override Task<ResolvePatientIdByAliasIdResponse> ResolvePatientIdByAliasId(ResolvePatientIdByAliasIdRequest request,
        ServerCallContext context)
    {
        return base.ResolvePatientIdByAliasId(request, context);
    }

    public override async Task<EnsureSubjectRefResponse> EnsureSubjectRef(EnsureSubjectRefRequest request,
        ServerCallContext context)
    {
        // try
        // {
        //     if (!Guid.TryParse(request.UserId, out var userId))
        //     {
        //         throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user id format"));
        //     }
        //
        //     PersonSeedDto? seed = null;
        //     if (request.PersonSeed != null)
        //     {
        //         seed = MapToPersonSeed(request.PersonSeed);
        //     }
        //
        //     var command = new SeedSubjectRefCommand(userId, seed);
        //     var result = await sender.Send(command, context.CancellationToken);
        //
        //     return new EnsureSubjectRefResponse
        //     {
        //         SubjectRef = result.SubjectRef.ToString()
        //     };
        // }
        // catch (Exception ex) when (!(ex is RpcException))
        // {
        //     logger.LogError(ex, "Error ensuring subject ref for user {UserId}", request.UserId);
        //     throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        // }

        throw new NotImplementedException();
    }

    // private static PersonSeedDto MapToPersonSeed(global::Pii.API.Protos.PersonSeedDto dto)
    // {
    //     DateOnly birthDate = DateOnly.FromDateTime(dto.BirthDate.ToDateTime());
    //
    //     UserGender gender = UserGender.Else;
    //     if (dto.Gender != 0 && Enum.IsDefined(typeof(UserGender), dto.Gender))
    //     {
    //         gender = (UserGender)dto.Gender;
    //     }
    //
    //     BuildingBlocks.Data.Common.ContactInfo contactInfo = dto.ContactInfo is not null
    //         ? new BuildingBlocks.Data.Common.ContactInfo
    //         {
    //             Address = dto.ContactInfo.Address,
    //             PhoneNumber = dto.ContactInfo.PhoneNumber,
    //             Email = dto.ContactInfo.Email
    //         }
    //         : new BuildingBlocks.Data.Common.ContactInfo();
    //
    //     
    //     //TODO quay lại sửa
    //     return new PersonSeedDto(
    //         Guid.NewGuid(),
    //         dto.FullName,
    //         gender,
    //         birthDate,
    //         contactInfo
    //     );
    // }
}