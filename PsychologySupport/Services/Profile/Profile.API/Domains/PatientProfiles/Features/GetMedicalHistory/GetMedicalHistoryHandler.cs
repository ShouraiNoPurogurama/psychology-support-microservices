using Profile.API.Data.Public;
using Profile.API.Domains.MentalDisorders.Dtos;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Exceptions;

namespace Profile.API.Domains.PatientProfiles.Features.GetMedicalHistory;

public record GetMedicalHistoryQuery(Guid PatientId) : IQuery<GetMedicalHistoryResult>;

public record GetMedicalHistoryResult(MedicalHistoryDto History);

public class GetMedicalHistoryHandler : IRequestHandler<GetMedicalHistoryQuery, GetMedicalHistoryResult>
{
    private readonly ProfileDbContext _context;

    public GetMedicalHistoryHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<GetMedicalHistoryResult> Handle(GetMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.MedicalHistory)
            .ThenInclude(mh => mh.SpecificMentalDisorders)
            .ThenInclude(mp => mp.MentalDisorder)
            .Include(p => p.MedicalHistory)
            .ThenInclude(mh => mh.PhysicalSymptoms)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient is null) throw new ProfileNotFoundException(request.PatientId);

        if (patient.MedicalHistory is null) throw new KeyNotFoundException($"Không tìm thấy hồ sơ y tế cho người dùng {request.PatientId}");

        var medicalHistoryDto = new MedicalHistoryDto(
            patient.MedicalHistory.Description,
            patient.MedicalHistory.DiagnosedAt,
            patient.MedicalHistory.SpecificMentalDisorders
                .Select(md => new SpecificMentalDisorderDto(md.Id, md.MentalDisorder.Name, md.Name, md.Description))
                .ToList(),
            patient.MedicalHistory.PhysicalSymptoms
                .Select(ps => new PhysicalSymptomDto(ps.Id, ps.Name, ps.Description))
                .ToList()
        );

        return new GetMedicalHistoryResult(medicalHistoryDto);
    }
}