using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.EventHandlers
{

    public class CreatePatientProfileHandler : IConsumer<CreatePatientProfileRequest>
    {
        private readonly ProfileDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreatePatientProfileHandler(ProfileDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<CreatePatientProfileRequest> context)
        {
            var request = context.Message;

            try
            {
                var newProfile = new PatientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    FullName = request.FullName,
                    Gender = request.Gender,
                    Allergies = request.Allergies,
                    PersonalityTraits = request.PersonalityTraits,
                    ContactInfo = request.ContactInfo
                };

                 _dbContext.PatientProfiles.Add(newProfile);
                await _dbContext.SaveChangesAsync();

                await context.RespondAsync(new CreatePatientProfileResponse(
                    newProfile.Id,
                    true,
                    "Patient profile created successfully."
                ));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(new CreatePatientProfileResponse(
                    Guid.Empty,
                    false,
                    $"Failed to create patient profile: {ex.Message}"
                ));
                return;
            }

            await _publishEndpoint.Publish(new SendEmailIntegrationEvent(request.ContactInfo.Email, "Patient Profile Created!",
            "Your profile has been created successfully."));

        }
    }

}
