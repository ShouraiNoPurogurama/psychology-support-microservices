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
                
                await _publishEndpoint.Publish(new SendEmailIntegrationEvent(
                    request.ContactInfo.Email,
                    "Hồ sơ EmoEase của bạn đã được tạo thành công",
                    "Chúc mừng! Hồ sơ người dùng của bạn đã được tạo trên hệ thống EmoEase. Hãy đăng nhập để cập nhật thêm thông tin nếu cần."));

                
                await context.RespondAsync(new CreatePatientProfileResponse(
                    newProfile.Id,
                    true,
                    "Hồ sơ người dùng đã được tạo thành công."
                ));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(new CreatePatientProfileResponse(
                    Guid.Empty,
                    false,
                    $"Không thể tạo hồ sơ người dùng: {ex.Message}"));
                return;
            }
        }
    }

}
