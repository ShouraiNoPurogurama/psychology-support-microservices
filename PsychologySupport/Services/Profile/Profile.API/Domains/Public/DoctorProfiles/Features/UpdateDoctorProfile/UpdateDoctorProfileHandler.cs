using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using Profile.API.Domains.DoctorProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Exceptions;

namespace Profile.API.Domains.DoctorProfiles.Features.UpdateDoctorProfile;

public record UpdateDoctorProfileCommand(Guid Id, UpdateDoctorProfileDto DoctorProfileDto) : ICommand<UpdateDoctorProfileResult>;

public record UpdateDoctorProfileResult(Guid Id);

public class UpdateDoctorProfileHandler : ICommandHandler<UpdateDoctorProfileCommand, UpdateDoctorProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateDoctorProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UpdateDoctorProfileResult> Handle(UpdateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles
            .Include(d => d.Specialties)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new ProfileNotFoundException();


        var specialties = await _context.Specialties
            .Where(s => request.DoctorProfileDto.SpecialtyIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        doctorProfile.Update(
            request.DoctorProfileDto.FullName ?? doctorProfile.FullName,
            request.DoctorProfileDto.Gender ?? doctorProfile.Gender,
            request.DoctorProfileDto.ContactInfo ?? doctorProfile.ContactInfo,
            specialties,
            request.DoctorProfileDto.Qualifications ?? doctorProfile.Qualifications,
            request.DoctorProfileDto.YearsOfExperience ?? doctorProfile.YearsOfExperience,
            request.DoctorProfileDto.Bio ?? doctorProfile.Bio
        );

        await _context.SaveChangesAsync(cancellationToken);

        var doctorProfileUpdatedEvent = new DoctorProfileUpdatedIntegrationEvent(
            doctorProfile.UserId,
            doctorProfile.FullName,
            doctorProfile.Gender,
            doctorProfile.ContactInfo.Email,
            doctorProfile.ContactInfo.PhoneNumber,
            doctorProfile.LastModified
        );

        await _publishEndpoint.Publish(doctorProfileUpdatedEvent, cancellationToken);

        return new UpdateDoctorProfileResult(doctorProfile.Id);
    }
}