using Grpc.Core;
using Notification.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Models.Pii;
using Profile.API.Models.Public;
using Profile.API.Protos;
using ContactInfo = BuildingBlocks.Data.Common.ContactInfo;

namespace Profile.API.Domains.Public.PatientProfiles.Services
{
    public class PatientProfileService : Protos.PatientProfileService.PatientProfileServiceBase
    {
        private readonly ProfileDbContext _dbContext;
        private readonly PiiDbContext _piiDbContext;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService.NotificationServiceClient _notiClient;

        public PatientProfileService(ProfileDbContext dbContext, IWebHostEnvironment env,
            NotificationService.NotificationServiceClient notiClient, PiiDbContext piiDbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _notiClient = notiClient ?? throw new ArgumentNullException(nameof(notiClient));
            _piiDbContext = piiDbContext;
        }

        public override async Task<CreatePatientProfileResponse> CreatePatientProfile(CreatePatientProfileRequest request,
            ServerCallContext context)
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
                        Message = "Reference không hợp lệ."
                    };
                }

                if (!Guid.TryParse(request.JobId, out var jobId))
                {
                    return new CreatePatientProfileResponse
                    {
                        Id = Guid.Empty.ToString(),
                        Success = false,
                        Message = "Công việc được chọn không hợp lệ."
                    };
                }

                // Map Protos.ContactInfo to BuildingBlocks.Data.Common.ContactInfo
                var contactInfo = ContactInfo.Of(request.ContactInfo.Address, request.ContactInfo.Email, request.ContactInfo.PhoneNumber);


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
                            Message = "Giới tính được chọn không hợp lệ."
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
                            Message = "Đặc điểm tính cách không hợp lệ."
                        };
                }

                //TODO quay lại sửa sau

                // Check if profile already exists
                //Bước 1: kiểm tra userId đã tồn tại ở PII chưa
                var piiProfile = await _piiDbContext.PersonProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                //TODO tí quay lại sửa
                if (piiProfile != null)
                {
                    //Bước 2: kiểm tra userId đã tồn tại ở PatientProfiles chưa
                    // var patientProfile = await _dbContext.PatientProfiles
                    //     .AsNoTracking()
                    //     .FirstOrDefaultAsync(p => p.UserId == userId);
                    //
                    // if (patientProfile != null)
                    // {
                    //     return new CreatePatientProfileResponse
                    //     {
                    //         Id = patientProfile.Id.ToString(),
                    //         Success = false,
                    //         Message = "Hồ sơ người dùng đã tồn tại."
                    //     };
                    // }
                }

                // Create new profile
                var newProfile = PatientProfile.Create(userId, "", personalityTrait, Guid.Parse(request.JobId), null);

                var newPiiProfile = PersonProfile.CreateActive(
                    subjectRef: Guid.NewGuid(),
                    userId: userId,
                    fullName: request.FullName,
                    gender: gender,
                    contactInfo: contactInfo,
                    birthDate: request.BirthDate != null
                        ? DateOnly.FromDateTime(request.BirthDate.ToDateTime())
                        : null
                );

                //TODO tí quay lại sửa
                var newAliasId = Guid.NewGuid();
                var newAliasOwner = new AliasOwnerMap
                {
                    Id = Guid.NewGuid(),
                    AliasId = newAliasId,
                    // UserId = newProfile.UserId
                };
                
                _piiDbContext.AliasOwnerMaps.Add(newAliasOwner);
                _piiDbContext.PersonProfiles.Add(newPiiProfile);
                await _piiDbContext.SaveChangesAsync();

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
                ["Year"] = DateTimeOffset.UtcNow.Year.ToString()
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