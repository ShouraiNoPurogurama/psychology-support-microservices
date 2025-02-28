using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.UpdatePatientProfile
{
    public record UpdatePatientProfileCommand(Guid Id, UpdatePatientProfileDto PatientProfileUpdate) : ICommand<UpdatePatientProfileResult>;

    public record UpdatePatientProfileResult(Guid Id);

    public class UpdatePatientProfileHandler : ICommandHandler<UpdatePatientProfileCommand, UpdatePatientProfileResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdatePatientProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UpdatePatientProfileResult> Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
        {
            var patientProfile = await _context.PatientProfiles.FindAsync([request.Id], cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException("Patient profile not found.");

            var dto = request.PatientProfileUpdate;
            patientProfile.Update(dto.FullName, dto.Gender, dto.Allergies, dto.PersonalityTraits, dto.ContactInfo);

            await _context.SaveChangesAsync(cancellationToken);

            var patientProfileUpdatedEvent = new PatientProfileUpdatedIntegrationEvent(
                patientProfile.UserId,
                patientProfile.FullName,
                patientProfile.Gender,
                patientProfile.ContactInfo.Email,
                patientProfile.ContactInfo.PhoneNumber
            );

            await _publishEndpoint.Publish(patientProfileUpdatedEvent, cancellationToken);

            return new UpdatePatientProfileResult(patientProfile.Id);
        }
    }
}
