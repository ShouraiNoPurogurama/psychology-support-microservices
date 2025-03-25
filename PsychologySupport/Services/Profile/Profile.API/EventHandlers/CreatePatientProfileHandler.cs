using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.EventHandlers
{

    public class CreatePatientProfileHandler : IConsumer<CreatePatientProfileRequest>
    {
        private readonly ProfileDbContext _dbContext;

        public CreatePatientProfileHandler(ProfileDbContext dbContext)
        {
            _dbContext = dbContext;
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

                await _dbContext.PatientProfiles.AddAsync(newProfile);
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
            }
        }
    }

}
