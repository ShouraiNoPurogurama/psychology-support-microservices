using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.CreatePatientProfile;

public record CreatePatientProfileCommand(CreatePatientProfileDto PatientProfileCreate) : ICommand<CreatePatientProfileResult>;

public record CreatePatientProfileResult(Guid Id);

public class CreatePatientProfileHandler : ICommandHandler<CreatePatientProfileCommand, CreatePatientProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePatientProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreatePatientProfileResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.PatientProfileCreate;

            if (_context.PatientProfiles.Any(p => p.UserId == dto.UserId))
                throw new BadRequestException("Hồ sơ người dùng này đã tồn tại trong hệ thống.");

            var patientProfile = PatientProfile.Create(
                dto.UserId,
                dto.FullName,
                dto.Gender,
                dto.Allergies,
                dto.PersonalityTraits,
                dto.ContactInfo,
                dto.JobId,
                dto.BirthDate
            );

            _context.PatientProfiles.Add(patientProfile);
            await _context.SaveChangesAsync(cancellationToken);

            var patientProfileCreatedEvent = new PatientProfileCreatedIntegrationEvent(
                patientProfile.UserId,
                patientProfile.FullName,
                patientProfile.Gender,
                patientProfile.ContactInfo.PhoneNumber,
                patientProfile.ContactInfo.Email,
                "Hồ sơ EmoEase của bạn đã được tạo thành công",
                "Chúc mừng! Hồ sơ bệnh nhân của bạn đã được tạo trên hệ thống EmoEase. Hãy đăng nhập để cập nhật thêm thông tin nếu cần.");

            await _publishEndpoint.Publish(patientProfileCreatedEvent, cancellationToken);

            return new CreatePatientProfileResult(patientProfile.Id);
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"Lỗi kết nối database: {ex.InnerException?.Message}", ex);
        }
    }
}