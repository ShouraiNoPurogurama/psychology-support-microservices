using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;

namespace Profile.API.Domains.Pii.Services;

public class PiiLocalService(PiiDbContext piiDbContext)
{
    public async Task<SimplePiiProfileDto> GetPiiProfileBySubjectRefAsync(string subjectRef, CancellationToken ct)
    {
        // 1. Chuyển string sang Guid (Rất quan trọng cho index)
        if (!Guid.TryParse(subjectRef, out var subjectRefGuid))
        {
            return null; 
        }

        // 2. Viết lại truy vấn (ĐƠN GIẢN HƠN RẤT NHIỀU)
        var query = from profile in piiDbContext.PersonProfiles
            where profile.SubjectRef == subjectRefGuid 
            select new SimplePiiProfileDto(
                profile.BirthDate ?? new DateOnly(2000,01,01),
                profile.Gender.ToString()
            );
        
        var result = await query.FirstOrDefaultAsync(ct);

        return result;
    }
}