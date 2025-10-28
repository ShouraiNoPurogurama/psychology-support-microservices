using Profile.API.Data.Pii;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Services;

public class PatientProfileLocalService(ProfileDbContext profileDbContext, PiiDbContext piiDbContext)
{
    // Đổi tên tham số từ aliasId -> subjectRef
    public async Task<SimplePatientProfileDto> GetPublicProfileBySubjectRefAsync(string subjectRef, CancellationToken ct)
    {
        if (!Guid.TryParse(subjectRef, out var subjectRefGuid))
        {
            return null; // Handle Guid không hợp lệ
        }
        
        // --- BƯỚC 1: Lấy PatientProfileId (Đã đơn giản hóa) ---
        // Bỏ join với AliasOwnerMaps, chỉ cần tìm trong PatientOwnerMaps
        var patientProfileId = await piiDbContext.PatientOwnerMaps
            .Where(p => p.SubjectRef == subjectRefGuid) // <-- Lọc trực tiếp
            .Select(p => p.PatientProfileId) // <-- Chỉ cần select
            .FirstOrDefaultAsync(cancellationToken: ct);
        
        // Nếu không có patient profile nào được map, trả về null
        if (patientProfileId == Guid.Empty)
        {
            return null;
        }

        // --- BƯỚC 2: Lấy Job (Giữ nguyên) ---
        var patientProfile = await profileDbContext.PatientProfiles
            .Include(p => p.Job)
            .Where(p => p.Id == patientProfileId)
            .Select(p => new SimplePatientProfileDto( p.Job!.JobTitle))
            .FirstOrDefaultAsync(cancellationToken: ct);

        return patientProfile;
    }
}