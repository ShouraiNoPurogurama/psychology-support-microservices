using Profile.API.Domains.Public.PatientProfiles.Dtos;
using Profile.API.Domains.Public.PatientProfiles.Exceptions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.PatchPatientProfile;

public record PatchPatientProfileCommand(Guid PatientProfileId, PatchPatientProfileDto PatientProfileUpdate)
    : ICommand<PatchPatientProfileResult>;

public record PatchPatientProfileResult(bool IsSuccess);

public class PatchPatientProfileHandler : ICommandHandler<PatchPatientProfileCommand, PatchPatientProfileResult>
{
    private readonly ProfileDbContext _context;

    public PatchPatientProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<PatchPatientProfileResult> Handle(PatchPatientProfileCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
                                 .FirstOrDefaultAsync(p => p.Id == request.PatientProfileId, cancellationToken: cancellationToken)
                             ?? throw new ProfileNotFoundException();

        var dto = request.PatientProfileUpdate;
        
        patientProfile.Update(
            dto.Allergies ?? patientProfile.Allergies,
            dto.PersonalityTraits ?? patientProfile.PersonalityTraits,
            dto.JobId ?? patientProfile.JobId,
            null
        );

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;
        
        return new PatchPatientProfileResult(result);
    }
}