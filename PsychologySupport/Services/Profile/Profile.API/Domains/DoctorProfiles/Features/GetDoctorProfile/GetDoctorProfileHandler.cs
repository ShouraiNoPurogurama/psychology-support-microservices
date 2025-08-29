using Mapster;
using Profile.API.Data.Public;
using Profile.API.Domains.DoctorProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Exceptions;

namespace Profile.API.Domains.DoctorProfiles.Features.GetDoctorProfile;

public record GetDoctorProfileQuery(Guid Id) : IQuery<GetDoctorProfileResult>;

public record GetDoctorProfileResult(DoctorProfileDto DoctorProfileDto);

public class GetDoctorProfileHandler(ProfileDbContext context) : IQueryHandler<GetDoctorProfileQuery, GetDoctorProfileResult>
{
    public async Task<GetDoctorProfileResult> Handle(GetDoctorProfileQuery request, CancellationToken cancellationToken)
    {
        var doctorProfile = await context.DoctorProfiles
                                .Include(d => d.MedicalRecords)
                                .Include(d => d.Specialties)
                                .FirstOrDefaultAsync(d => d.Id.Equals(request.Id), cancellationToken)
                            ?? throw new ProfileNotFoundException("Doctor profile", request.Id);

        var doctorProfileDto = doctorProfile.Adapt<DoctorProfileDto>();

        return new GetDoctorProfileResult(doctorProfileDto);
    }
}