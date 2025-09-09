using Profile.API.Domains.Public.PatientProfiles.Dtos;
using Profile.API.Domains.Public.PatientProfiles.Exceptions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.PatchPatientProfile;

public record UpdatePatientProfileCommand(Guid SubjectRef, UpdatePatientProfileDto PatientProfileUpdate)
    : ICommand<UpdatePatientProfileResult>;

public record UpdatePatientProfileResult(bool IsSuccess);

public class PatchPatientProfileHandler : ICommandHandler<UpdatePatientProfileCommand, UpdatePatientProfileResult>
{
    private readonly ProfileDbContext _context;

    public PatchPatientProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<UpdatePatientProfileResult> Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
                                 .FirstOrDefaultAsync(p => p.SubjectRef == request.SubjectRef, cancellationToken: cancellationToken)
                             ?? throw new ProfileNotFoundException();

        var dto = request.PatientProfileUpdate;
        
        patientProfile.Update(
            dto.Allergies ?? patientProfile.Allergies,
            dto.PersonalityTraits ?? patientProfile.PersonalityTraits,
            dto.JobId ?? patientProfile.JobId
        );

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;
        
        return new UpdatePatientProfileResult(result);
    }
}