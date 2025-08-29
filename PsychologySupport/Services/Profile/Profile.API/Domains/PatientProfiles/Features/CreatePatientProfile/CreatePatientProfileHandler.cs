using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Models;

namespace Profile.API.Domains.PatientProfiles.Features.CreatePatientProfile;

public record CreatePatientProfileCommand(CreatePatientProfileDto PatientProfileCreate) : ICommand<CreatePatientProfileResult>;

public record CreatePatientProfileResult(Guid Id);

public class CreatePatientProfileHandler : ICommandHandler<CreatePatientProfileCommand, CreatePatientProfileResult>
{
    private readonly ProfileDbContext _publicDbContext;
    private readonly PiiDbContext _piiDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePatientProfileHandler(ProfileDbContext publicDbContext, IPublishEndpoint publishEndpoint, PiiDbContext piiDbContext)
    {
        _publicDbContext = publicDbContext;
        _publishEndpoint = publishEndpoint;
        _piiDbContext = piiDbContext;
    }

    public async Task<CreatePatientProfileResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.PatientProfileCreate;

            var profileExisted = await (
                from alias in _piiDbContext.AliasOwnerMaps
                join patient in _publicDbContext.PatientProfiles
                    on alias.UserId equals patient.UserId
                where alias.AliasId == dto.AliasId
                select patient.Id
            ).AnyAsync(cancellationToken);

            if (profileExisted)
                throw new BadRequestException("Hồ sơ người dùng này đã tồn tại trong hệ thống.");

            var ownerMap = await _piiDbContext.AliasOwnerMaps.AsNoTracking()
                .Where(a => a.AliasId == dto.AliasId)
                .Include(a => a.PersonProfile)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                         ?? throw new NotFoundException("Không tìm thấy hồ sơ gốc của người dùng này trong hệ thống.");
            
            var personProfile = ownerMap.PersonProfile;
            
            var patientProfile = PatientProfile.Create(
                personProfile.UserId,
                dto.FullName,
                dto.Gender,
                dto.Allergies,
                dto.PersonalityTraits,
                dto.ContactInfo,
                dto.JobId,
                dto.BirthDate
            );
            
            _publicDbContext.PatientProfiles.Add(patientProfile);
            await _publicDbContext.SaveChangesAsync(cancellationToken);

            var patientProfileCreatedEvent = new PatientProfileCreatedIntegrationEvent(
                patientProfile.UserId,
                personProfile.FullName!,
                personProfile.Gender,
                personProfile.ContactInfo.PhoneNumber!,
                personProfile.ContactInfo.Email,
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