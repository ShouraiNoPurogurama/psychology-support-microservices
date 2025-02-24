using Profile.API.Common.ValueObjects;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.DoctorProfiles.Models;


namespace Profile.API.DoctorProfiles.Features.CreateDoctorProfile
{
    public record CreateDoctorProfileCommand(CreateDoctorProfileDto DoctorProfile) : ICommand<CreateDoctorProfileResult>;

    public record CreateDoctorProfileResult(Guid Id);

    public class CreateDoctorProfileHandler : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public CreateDoctorProfileHandler(ProfileDbContext context,IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
        {
            var doctorProfileCreate = request.DoctorProfile;

            var doctorProfile = DoctorProfile.Create(
                doctorProfileCreate.UserId,
                doctorProfileCreate.FullName,
                doctorProfileCreate.Gender.ToString(),
                new ContactInfo(
                    doctorProfileCreate.ContactInfo.Address,
                    doctorProfileCreate.ContactInfo.PhoneNumber,
                    doctorProfileCreate.ContactInfo.Email
                ),
                doctorProfileCreate.Specialty,
                doctorProfileCreate.Qualifications,
                doctorProfileCreate.YearsOfExperience,
                doctorProfileCreate.Bio
            );

            doctorProfile.CreatedAt = DateTimeOffset.UtcNow;

            _context.DoctorProfiles.Add(doctorProfile);
            await _context.SaveChangesAsync(cancellationToken);

            var doctorProfileCreatedEvent = new DoctorProfileCreatedIntegrationEvent(
                doctorProfile.UserId,
                doctorProfile.Gender,
                doctorProfile.ContactInfo.Email,
                doctorProfile.ContactInfo.PhoneNumber,
                doctorProfile.CreatedAt
            );

            await _publishEndpoint.Publish(doctorProfileCreatedEvent, cancellationToken);

            return new CreateDoctorProfileResult(doctorProfile.Id);
        }

    }
}
