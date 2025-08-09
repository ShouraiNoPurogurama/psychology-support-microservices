using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.PatientProfiles.Models;
using System.IO;

namespace Profile.API.EventHandlers
{

    public class CreatePatientProfileHandler : IConsumer<CreatePatientProfileRequest>
    {
        private readonly ProfileDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IWebHostEnvironment _env;

        public CreatePatientProfileHandler(ProfileDbContext dbContext, IPublishEndpoint publishEndpoint, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _env = env;
        }

        public async Task Consume(ConsumeContext<CreatePatientProfileRequest> context)
        {
            var request = context.Message;

            try
            {
                var existingProfile = await _dbContext.PatientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == request.UserId ||
                                              p.ContactInfo.Email == request.ContactInfo.Email);

                if (existingProfile is not null)
                {
                    await context.RespondAsync(new CreatePatientProfileResponse(
                        existingProfile.Id,
                        false,
                        "Hồ sơ đã tồn tại."));
                    return;
                }

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

                var welcomeTemplatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "welcomepatient.html");

                var welcomeBody = RenderTemplate(welcomeTemplatePath, new Dictionary<string, string>
                {
                    ["LoginUrl"] = "https://www.emoease.vn/EMO/learnAboutEmo",
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                });

                await _publishEndpoint.Publish(new SendEmailIntegrationEvent(
                    request.ContactInfo.Email,
                    "Hồ sơ EmoEase của bạn đã được tạo thành công",
                    welcomeBody));

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
            }
        }

        private string RenderTemplate(string templatePath, Dictionary<string, string> values)
        {
            var template = File.ReadAllText(templatePath);
            foreach (var pair in values)
            {
                template = template.Replace($"{{{{{pair.Key}}}}}", pair.Value);
            }
            return template;
        }
    }

}
