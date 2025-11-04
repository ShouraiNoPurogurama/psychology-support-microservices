using Profile.API.Data.Pii;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Services;

public class PatientProfileLocalService(
    ILogger<PatientProfileLocalService> logger,
    ProfileDbContext profileDbContext, 
    PiiDbContext piiDbContext)
{
    // Đổi tên tham số từ aliasId -> subjectRef
    public async Task<SimplePatientProfileDto> GetPublicProfileBySubjectRefAsync(string subjectRef, CancellationToken ct)
    {
        if (!Guid.TryParse(subjectRef, out var subjectRefGuid))
        {
            logger.LogCritical("SubjectRef không hợp lệ: {SubjectRef}", subjectRef);
            return null; 
        }
        
        // --- BƯỚC 1: Lấy PatientProfileId (Đã đơn giản hóa) ---
        var patientProfileId = await piiDbContext.PatientOwnerMaps
            .Where(p => p.SubjectRef == subjectRefGuid) 
            .Select(p => p.PatientProfileId) 
            .FirstOrDefaultAsync(cancellationToken: ct);
        
        if (patientProfileId == Guid.Empty)
        {
            logger.LogCritical("PatientProfileId không tìm thấy cho SubjectRef: {SubjectRef}", subjectRef);
            return null;
        }

        var patientProfile = await profileDbContext.PatientProfiles
            .Include(p => p.Job)
            .Where(p => p.Id == patientProfileId)
            .FirstOrDefaultAsync(cancellationToken: ct);

        var profileDto = new SimplePatientProfileDto(patientProfile?.Job?.JobTitle ?? string.Empty);

        return profileDto;
    }
}