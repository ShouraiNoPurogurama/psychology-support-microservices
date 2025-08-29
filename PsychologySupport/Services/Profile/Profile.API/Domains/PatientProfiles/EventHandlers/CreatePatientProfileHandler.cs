using System.Transactions;
using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Models;
using Profile.API.Domains.Pii.Models;

namespace Profile.API.Domains.PatientProfiles.EventHandlers
{
    public class CreatePatientProfileHandler : IConsumer<CreatePatientProfileRequest>
    {
        private readonly ProfileDbContext _profileDbContext;
        private readonly PiiDbContext _piiDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IWebHostEnvironment _env;

        public CreatePatientProfileHandler(ProfileDbContext profileDbContext, IPublishEndpoint publishEndpoint,
            IWebHostEnvironment env, PiiDbContext piiDbContext)
        {
            _profileDbContext = profileDbContext;
            _publishEndpoint = publishEndpoint;
            _env = env;
            _piiDbContext = piiDbContext;
        }

        public async Task Consume(ConsumeContext<CreatePatientProfileRequest> context)
        {
            var request = context.Message;
            var ct = context.CancellationToken;

            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var existingPii = await _piiDbContext.PersonProfiles
                    .FirstOrDefaultAsync(p => p.UserId == request.UserId ||
                                              p.ContactInfo.Email == request.ContactInfo.Email, cancellationToken: ct);

                if (existingPii is not null)
                {
                    var existedPatient = await _profileDbContext.PatientProfiles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UserId == existingPii.UserId, ct);

                    if (existedPatient is not null)
                    {
                        scope.Complete();
                        await context.RespondAsync(new CreatePatientProfileResponse(
                            existedPatient.Id, false, "Hồ sơ đã tồn tại."));
                        return;
                    }
                    
                    //Chỉ tạo PatientProfile mới, KHÔNG tạo PII/alias nữa
                    var ppId = Guid.NewGuid();
                    var newPatientOnly = new PatientProfile
                    {
                        Id = ppId,
                        UserId = existingPii.UserId,
                        Allergies = request.Allergies,
                        PersonalityTraits = request.PersonalityTraits,
                    };
                    _profileDbContext.PatientProfiles.Add(newPatientOnly);

                    await _profileDbContext.SaveChangesAsync(ct);
                    await _piiDbContext.SaveChangesAsync(ct);
                    scope.Complete();

                    await context.RespondAsync(new CreatePatientProfileResponse(
                        newPatientOnly.Id, true, "Đã tạo hồ sơ người dùng."));
                    return;
                }

                //Tạo mới cả PII + Patient + Alias
                var newProfile = new PatientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Allergies = request.Allergies,
                    PersonalityTraits = request.PersonalityTraits,
                };

                var newPiiProfile = PersonProfile.Create(
                    userId: request.UserId,
                    fullName: request.FullName,
                    gender: request.Gender,
                    contactInfo: request.ContactInfo,
                    birthDate: request.BirthDate
                );

                var newAliasId = Guid.NewGuid();
                var newAliasOwner = new AliasOwnerMap
                {
                    Id = Guid.NewGuid(),
                    AliasId = newAliasId,
                    UserId = request.UserId
                };

                _piiDbContext.PersonProfiles.Add(newPiiProfile);
                _profileDbContext.PatientProfiles.Add(newProfile);
                _piiDbContext.AliasOwnerMaps.Add(newAliasOwner);

                await _profileDbContext.SaveChangesAsync(ct);
                await _piiDbContext.SaveChangesAsync(ct);

                scope.Complete();

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