using Mapster;
using Profile.API.Exceptions;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetPatientProfile;

public record GetPatientProfileQuery(Guid Id) : IQuery<GetPatientProfileResult>;

public record GetPatientProfileResult(GetPatientProfileDto PatientProfileDto);

public class GetPatientProfileHandler(ProfileDbContext context) : IQueryHandler<GetPatientProfileQuery, GetPatientProfileResult>
{
    public async Task<GetPatientProfileResult> Handle(GetPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var patientProfile = await context.PatientProfiles
                                 .Include(p => p.Job)
                                 .Include(p => p.MedicalHistory)
                                 .ThenInclude(m => m.PhysicalSymptoms)
                                 .Include(p => p.MedicalHistory)
                                 .ThenInclude(m => m.SpecificMentalDisorders)
                                 .Include(p => p.MedicalRecords)
                                 .ThenInclude(m => m.SpecificMentalDisorders)
                                 .FirstOrDefaultAsync(p => p.Id.Equals(request.Id), cancellationToken)
                             ?? throw new ProfileNotFoundException("Patient profile", request.Id);

        var patientProfileDto = patientProfile.Adapt<GetPatientProfileDto>();

        return new GetPatientProfileResult(patientProfileDto);
    }
}