using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.CreatePatientProfile;

public record CreatePatientProfileCommand(CreatePatientProfileDto PatientProfileCreate) : ICommand<CreatePatientProfileResult>;

public record CreatePatientProfileResult(Guid Id);

public class CreatePatientProfileHandler : ICommandHandler<CreatePatientProfileCommand, CreatePatientProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePatientProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreatePatientProfileResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.PatientProfileCreate;

            if (_context.PatientProfiles.Any(p => p.UserId == dto.UserId))
                throw new BadRequestException("Patient profile already exists.");

            var patientProfile = PatientProfile.Create(
                dto.UserId,
                dto.FullName,
                dto.Gender,
                dto.Allergies,
                dto.PersonalityTraits,
                dto.ContactInfo,
                dto.JobId,
                dto.BirthDate
            );

            _context.PatientProfiles.Add(patientProfile);
            await _context.SaveChangesAsync(cancellationToken);

            var patientProfileCreatedEvent = new PatientProfileCreatedIntegrationEvent(
                patientProfile.UserId,
                patientProfile.FullName,
                patientProfile.Gender,
                patientProfile.ContactInfo.PhoneNumber,
                patientProfile.ContactInfo.Email,
                "Patient profile created successfully.",
                "Hello world"
            );

            await _publishEndpoint.Publish(patientProfileCreatedEvent, cancellationToken);

            return new CreatePatientProfileResult(patientProfile.Id);
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"Database error: {ex.InnerException?.Message}", ex);
        }
    }
}