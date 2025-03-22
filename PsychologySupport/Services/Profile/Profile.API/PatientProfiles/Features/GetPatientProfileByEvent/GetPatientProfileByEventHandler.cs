using BuildingBlocks.Enums;
using Mapster;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetPatientProfileByEvent;

public record GetPatientProfileByEventQuery(Guid PatientId, Guid? UserId = null) : IQuery<GetPatientProfileByEventResult>;

public record GetPatientProfileByEventResult(
    bool PatientExists,
    Guid Id,
    string FullName,
    UserGender Gender,
    string? Allergies,
    string PersonalityTraits,
    string Address,
    string PhoneNumber,
    string Email);

public class GetPatientProfileByEventHandler(ProfileDbContext dbContext)
    : IQueryHandler<GetPatientProfileByEventQuery, GetPatientProfileByEventResult>
{
    public async Task<GetPatientProfileByEventResult> Handle(GetPatientProfileByEventQuery request,
        CancellationToken cancellationToken)
    {
        PatientProfile? patientProfile = null;
        if (request.UserId is not null)
        {
            patientProfile = await dbContext.PatientProfiles.FirstOrDefaultAsync(x => x.UserId == request.UserId);

            if (patientProfile is null)
            {
                return new GetPatientProfileByEventResult(
                    false,
                    Guid.Empty,
                    string.Empty,
                    UserGender.Else,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);
            }

            return patientProfile.Adapt<GetPatientProfileByEventResult>() with
            {
                Email = patientProfile.ContactInfo.Email,
                Address = patientProfile.ContactInfo.Address,
                PhoneNumber = patientProfile.ContactInfo.PhoneNumber,
                PatientExists = true
            };
        }

        patientProfile = await dbContext.PatientProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PatientId);

        if (patientProfile is null)
        {
            return new GetPatientProfileByEventResult(
                false,
                Guid.Empty,
                string.Empty,
                UserGender.Else,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        return patientProfile.Adapt<GetPatientProfileByEventResult>() with
        {
            Email = patientProfile.ContactInfo.Email,
            Address = patientProfile.ContactInfo.Address,
            PhoneNumber = patientProfile.ContactInfo.PhoneNumber,
            PatientExists = true
        };
    }
}