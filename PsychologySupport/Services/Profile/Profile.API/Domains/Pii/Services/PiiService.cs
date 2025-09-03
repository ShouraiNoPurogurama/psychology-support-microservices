using Grpc.Core;
using Pii.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Features.EnsureSubjectRef;
using PersonSeedDto = Profile.API.Domains.Pii.Dtos.PersonSeedDto;
using UserGender = BuildingBlocks.Enums.UserGender;

namespace Profile.API.Domains.Pii.Services;

public class PiiService : global::Pii.API.Protos.PiiService.PiiServiceBase
{
    private readonly PiiDbContext _piiDbContext;
    private readonly ProfileDbContext _profileDbContext;
    private readonly ISender _sender;
    private readonly ILogger<PiiService> _logger;

    public PiiService(PiiDbContext piiDbContext, ProfileDbContext profileDbContext, ISender sender, ILogger<PiiService> logger)
    {
        _piiDbContext = piiDbContext;
        _profileDbContext = profileDbContext;
        _sender = sender;
        _logger = logger;
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


    public override async Task<ResolveSubjectRefByUserIdResponse> ResolveSubjectRefByUserId(
        ResolveSubjectRefByUserIdRequest request, ServerCallContext context)
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

    public override async Task<EnsureSubjectRefResponse> EnsureSubjectRef(EnsureSubjectRefRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user id format"));
            }

            PersonSeedDto? seed = null;
            if (request.PersonSeed != null)
            {
                seed = MapToPersonSeed(request.PersonSeed);
            }

            var command = new EnsureSubjectRefCommand(userId, seed);
            var result = await _sender.Send(command, context.CancellationToken);

            return new EnsureSubjectRefResponse
            {
                SubjectRef = result.SubjectRef.ToString()
            };
        }
        catch (Exception ex) when (!(ex is RpcException))
        {
            _logger.LogError(ex, "Error ensuring subject ref for user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    private static PersonSeedDto MapToPersonSeed(global::Pii.API.Protos.PersonSeedDto dto)
    {
        DateOnly birthDate = DateOnly.FromDateTime(dto.BirthDate.ToDateTime());

        UserGender gender = UserGender.Else;
        if (dto.Gender != 0 && Enum.IsDefined(typeof(UserGender), dto.Gender))
        {
            gender = (UserGender)dto.Gender;
        }

        BuildingBlocks.Data.Common.ContactInfo contactInfo = dto.ContactInfo is not null
            ? new BuildingBlocks.Data.Common.ContactInfo
            {
                Address = dto.ContactInfo.Address,
                PhoneNumber = dto.ContactInfo.PhoneNumber,
                Email = dto.ContactInfo.Email
            }
            : new BuildingBlocks.Data.Common.ContactInfo();

        return new PersonSeedDto(
            dto.FullName,
            gender,
            birthDate,
            contactInfo
        );
    }
}