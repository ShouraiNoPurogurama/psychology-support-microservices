using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using Profile.API.Domains.Public.DoctorProfiles.Dtos;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.DoctorProfiles.Features.CreateDoctorProfile;

public record CreateDoctorProfileCommand(CreateDoctorProfileDto DoctorProfile) : ICommand<CreateDoctorProfileResult>;

public record CreateDoctorProfileResult(Guid Id);

public class CreateDoctorProfileHandler(ProfileDbContext context,
    IRequestClient<DoctorProfileCreatedRequestEvent> requestClient)
    : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
{
    public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var dto = request.DoctorProfile;

        var doctorProfileCreatedEvent = new DoctorProfileCreatedRequestEvent(
            dto.FullName,
            dto.Gender,
            dto.ContactInfo.Email,
            dto.ContactInfo.PhoneNumber,
            "None"
        );

        // Send event and wait for response
        var response = await requestClient.GetResponse<DoctorProfileCreatedResponseEvent>(doctorProfileCreatedEvent, cancellationToken);

        if (!response.Message.Success)
        {
            throw new InvalidOperationException("Tạo hồ sơ bác sĩ không thành công: " + response.Message);
        }

        var doctorProfile = DoctorProfile.Create(
            response.Message.UserId, 
            dto.FullName,
            dto.Gender,
            ContactInfo.Of(
                dto.ContactInfo.Address,
                dto.ContactInfo.Email,
                dto.ContactInfo.PhoneNumber
            ),
            dto.Qualifications,
            dto.YearsOfExperience,
            dto.Bio
        );

        context.DoctorProfiles.Add(doctorProfile);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateDoctorProfileResult(response.Message.UserId);
    }
}
