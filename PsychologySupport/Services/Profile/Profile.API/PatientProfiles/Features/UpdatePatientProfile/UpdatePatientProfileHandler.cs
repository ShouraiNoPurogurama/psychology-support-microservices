using BuildingBlocks.Data.Common;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.UpdatePatientProfile;

public record UpdatePatientProfileCommand(Guid Id, UpdatePatientProfileDto PatientProfileUpdate)
    : ICommand<UpdatePatientProfileResult>;

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
        var patientProfile = await _context.PatientProfiles.FindAsync([request.Id], cancellationToken)
                             ?? throw new KeyNotFoundException("Patient profile not found.");

        var dto = request.PatientProfileUpdate;
        
        var phoneNumber = string.IsNullOrWhiteSpace(dto.ContactInfo?.PhoneNumber)
            ? patientProfile.ContactInfo.PhoneNumber
            : dto.ContactInfo.PhoneNumber;
        
        var email = string.IsNullOrWhiteSpace(dto.ContactInfo?.Email)
            ? patientProfile.ContactInfo.Email
            : dto.ContactInfo.Email;
        
        var address = string.IsNullOrWhiteSpace(dto.ContactInfo?.Address)
            ? patientProfile.ContactInfo.Address
            : dto.ContactInfo.Address;
        
        var newContactInfo = ContactInfo.Of(
            address,
            phoneNumber,
            email
        );
        
        patientProfile.Update(
            dto.FullName ?? patientProfile.FullName,
            dto.Gender ?? patientProfile.Gender,
            dto.Allergies ?? patientProfile.Allergies,
            dto.PersonalityTraits ?? patientProfile.PersonalityTraits,
            newContactInfo,
            dto.JobId ?? patientProfile.JobId,
            dto.BirthDate ?? patientProfile.BirthDate
        );

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