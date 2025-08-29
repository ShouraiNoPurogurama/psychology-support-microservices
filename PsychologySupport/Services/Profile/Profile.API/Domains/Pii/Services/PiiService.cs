using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pii.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Enum = System.Enum;

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

    public override async Task<ResolveUserIdResponse> ResolveUserId(ResolveUserIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
        {
            return new ResolveUserIdResponse { UserId = Guid.Empty.ToString() };
        }

        var existingOwnerMap = await _piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AliasId == aliasId);

        var userId = existingOwnerMap?.UserId ?? Guid.Empty;

        return new ResolveUserIdResponse { UserId = userId.ToString() };
    }

    public override async Task<ResolvePatientIdResponse> ResolvePatientId(ResolvePatientIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
        {
            return new ResolvePatientIdResponse { PatientId = Guid.Empty.ToString() };
        }

        var userId = await _piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .Where(a => a.AliasId == aliasId)
            .Select(a => a.UserId)
            .FirstOrDefaultAsync();

        var patientId = await _profileDbContext.PatientProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        return new ResolvePatientIdResponse { PatientId = patientId.ToString() };
    }

    public override async Task<ResolvePersonInfoResponse> ResolvePersonInfo(ResolvePersonInfoRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.AliasId, out var aliasId))
        {
            return new ResolvePersonInfoResponse
            {
                UserId = Guid.Empty.ToString(),
                FullName = string.Empty,
                Gender = UserGender.None,
                ContactInfo = null
            };
        }

        var ownerMap = await _piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .Include(a => a.PersonProfile)
            .FirstOrDefaultAsync(a => a.AliasId == aliasId);

        if (ownerMap?.PersonProfile == null)
        {
            return new ResolvePersonInfoResponse
            {
                UserId = Guid.Empty.ToString(),
                FullName = string.Empty,
                Gender = UserGender.None,
                ContactInfo = null
            };
        }

        var profile = ownerMap.PersonProfile;

        var contactInfo = new ContactInfo
        {
            Address = profile.ContactInfo.Address,
            Email = profile.ContactInfo.Email,
            PhoneNumber = profile.ContactInfo.PhoneNumber ?? string.Empty
        };

        var birthDate = profile.BirthDate.HasValue
            ? Timestamp.FromDateTime(profile.BirthDate.Value.ToDateTime(TimeOnly.MinValue).ToUniversalTime())
            : null;

        return new ResolvePersonInfoResponse
        {
            UserId = ownerMap.UserId.ToString(),
            FullName = profile.FullName ?? string.Empty,
            Gender = Enum.TryParse<UserGender>(profile.Gender.ToString(), out var gender) ? gender : UserGender.None,
            BirthDate = birthDate,
            ContactInfo = contactInfo
        };
    }
}
