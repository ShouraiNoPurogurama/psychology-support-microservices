using BuildingBlocks.CQRS;
using Mapster;
using Profile.API.Data;
using Profile.API.Dtos;
using Profile.API.Exceptions;

namespace Profile.API.Features.PatientProfiles.UpdatePatientProfile
{
    public record UpdatePatientProfileCommand(PatientProfileDto PatientProfile) : ICommand<UpdatePatientProfileResult>;

    public record UpdatePatientProfileResult(bool IsSuccess);

    public class UpdatePatientProfileHandler : ICommandHandler<UpdatePatientProfileCommand, UpdatePatientProfileResult>
    {
        private readonly ProfileDbContext _context;

        public UpdatePatientProfileHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<UpdatePatientProfileResult> Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
        {
            var existingProfile = await _context.PatientProfiles.FindAsync(request.PatientProfile.Id)
                                  ?? throw new ProfileNotFoundException("Patient Profile", request.PatientProfile.Id);

            existingProfile = request.PatientProfile.Adapt(existingProfile);
            existingProfile.LastModified = DateTime.UtcNow;

            _context.Update(existingProfile);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            return new UpdatePatientProfileResult(result);
        }
    }
}
