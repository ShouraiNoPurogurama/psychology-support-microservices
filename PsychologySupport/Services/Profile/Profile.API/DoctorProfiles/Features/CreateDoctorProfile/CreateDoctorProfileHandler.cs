using Profile.API.DoctorProfiles.Dtos;
using Profile.API.DoctorProfiles.Models;

namespace Profile.API.DoctorProfiles.Features.CreateDoctorProfile;

public record CreateDoctorProfileCommand(CreateDoctorProfileDto DoctorProfile) : ICommand<CreateDoctorProfileResult>;

public record CreateDoctorProfileResult(Guid Id);

public class CreateDoctorProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
{
    public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var dto = request.DoctorProfile;

        if (context.DoctorProfiles.Any(p => p.UserId == dto.UserId))
            throw new BadRequestException("Doctor profile already exists.");

        var doctorProfile = DoctorProfile.Create(
            dto.UserId,
            dto.FullName,
            dto.Gender,
            new ContactInfo(
                dto.ContactInfo.Address,
                dto.ContactInfo.PhoneNumber,
                dto.ContactInfo.Email
            ),
            dto.Specialties,
            dto.Qualifications,
            dto.YearsOfExperience,
            dto.Bio
        );

        context.DoctorProfiles.Add(doctorProfile);
        await context.SaveChangesAsync(cancellationToken);

        var doctorProfileCreatedEvent = new DoctorProfileCreatedIntegrationEvent(
            doctorProfile.UserId,
            doctorProfile.FullName,
            doctorProfile.Gender,
            doctorProfile.ContactInfo.Email,
            doctorProfile.ContactInfo.PhoneNumber
        );

        await publishEndpoint.Publish(doctorProfileCreatedEvent, cancellationToken);

        return new CreateDoctorProfileResult(doctorProfile.Id);
    }
}