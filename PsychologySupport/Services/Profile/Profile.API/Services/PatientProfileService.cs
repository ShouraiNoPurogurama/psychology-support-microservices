using Grpc.Core;
using Notification.API.Protos;
using Profile.API.PatientProfiles.Models;
using Profile.API.Protos;


namespace Profile.API.Services
{
    public class PatientProfileService : Protos.PatientProfileService.PatientProfileServiceBase
    {
        private readonly ProfileDbContext _dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService.NotificationServiceClient _notiClient;

        public PatientProfileService(ProfileDbContext dbContext, IWebHostEnvironment env,
                NotificationService.NotificationServiceClient notiClient
            )
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _notiClient = notiClient ?? throw new ArgumentNullException(nameof(notiClient));
        }

        public override async Task<CreatePatientProfileResponse> CreatePatientProfile(CreatePatientProfileRequest request, ServerCallContext context)
        {
            try
            {
                // Convert user_id from string to Guid
                if (!Guid.TryParse(request.UserId, out var userId))
                {
                    return new CreatePatientProfileResponse
                    {
                        Id = Guid.Empty.ToString(),
                        Success = false,
                        Message = "Invalid user ID."
                    };
                }

                // Map Protos.ContactInfo to BuildingBlocks.Data.Common.ContactInfo
                var contactInfo = new BuildingBlocks.Data.Common.ContactInfo
                {
                    Address = request.ContactInfo.Address,
                    Email = request.ContactInfo.Email,
                    PhoneNumber = request.ContactInfo.PhoneNumber
                };

                // Check gender
                BuildingBlocks.Enums.UserGender gender;
                switch (request.Gender)
                {
                    case Protos.UserGender.Male:
                        gender = BuildingBlocks.Enums.UserGender.Male;
                        break;
                    case Protos.UserGender.Female:
                        gender = BuildingBlocks.Enums.UserGender.Female;
                        break;
                    case Protos.UserGender.Else:
                        gender = BuildingBlocks.Enums.UserGender.Else;
                        break;
                    default:
                        return new CreatePatientProfileResponse
                        {
                            Id = Guid.Empty.ToString(),
                            Success = false,
                            Message = "Invalid gender."
                        };
                }

                // Check personality trait
                BuildingBlocks.Enums.PersonalityTrait personalityTrait;
                switch (request.PersonalityTrait)
                {
                    case Protos.PersonalityTrait.None1:
                        personalityTrait = BuildingBlocks.Enums.PersonalityTrait.None;
                        break;
                    case Protos.PersonalityTrait.Introversion:
                        personalityTrait = BuildingBlocks.Enums.PersonalityTrait.Introversion;
                        break;
                    case Protos.PersonalityTrait.Extroversion:
                        personalityTrait = BuildingBlocks.Enums.PersonalityTrait.Extroversion;
                        break;
                    case Protos.PersonalityTrait.Adaptability:
                        personalityTrait = BuildingBlocks.Enums.PersonalityTrait.Adaptability;
                        break;
                    default:
                        return new CreatePatientProfileResponse
                        {
                            Id = Guid.Empty.ToString(),
                            Success = false,
                            Message = "Invalid personality trait."
                        };
                }

                // Check if profile already exists
                var existingProfile = await _dbContext.PatientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId || p.ContactInfo.Email == contactInfo.Email);

                if (existingProfile != null)
                {
                    return new CreatePatientProfileResponse
                    {
                        Id = existingProfile.Id.ToString(),
                        Success = false,
                        Message = "Profile already exists."
                    };
                }

                // Create new profile
                var newProfile = new PatientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = request.FullName,
                    Gender = gender,
                    Allergies = request.Allergies,
                    PersonalityTraits = personalityTrait,
                    ContactInfo = contactInfo
                };

                _dbContext.PatientProfiles.Add(newProfile);
                await _dbContext.SaveChangesAsync();

                // Send welcome email
                await SendWelcomeEmailAsync(contactInfo.Email, request.FullName);

                return new CreatePatientProfileResponse
                {
                    Id = newProfile.Id.ToString(),
                    Success = true,
                    Message = "User profile created successfully."
                };
            }
            catch (Exception ex)
            {
                return new CreatePatientProfileResponse
                {
                    Id = Guid.Empty.ToString(),
                    Success = false,
                    Message = $"Unable to create user profile: {ex.Message}"
                };
            }
        }

        private async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "WelcomePatient.html");
            var body = RenderTemplate(templatePath, new Dictionary<string, string>
            {
                ["FullName"] = fullName,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            });

   
            var emailRequest = new Notification.API.Protos.SendEmailRequest
            {
                EventId = Guid.NewGuid().ToString(),
                To = email,
                Subject = "Welcome to Our Service",
                Body = body
            };

            await _notiClient.SendEmailAsync(emailRequest);
        }

        private string RenderTemplate(string templatePath, Dictionary<string, string> values)
        {
            var template = File.ReadAllText(templatePath);
            foreach (var pair in values)
            {
                template = template.Replace($"{{{{{pair.Key}}}}}", pair.Value ?? string.Empty);
            }
            return template;
        }
    }
}