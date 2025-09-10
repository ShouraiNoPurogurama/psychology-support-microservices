using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetMedicalRecord
{
    public record GetMedicalRecordQuery(Guid MedicalRecordId) : IRequest<GetMedicalRecordResult>;

    public record GetMedicalRecordResult(MedicalRecordDto Record);

    public class GetMedicalRecordHandler : IRequestHandler<GetMedicalRecordQuery, GetMedicalRecordResult>
    {
        private readonly ProfileDbContext _context;

        public GetMedicalRecordHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetMedicalRecordResult> Handle(GetMedicalRecordQuery request, CancellationToken cancellationToken)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.MedicalHistory)
                    .ThenInclude(mh => mh.SpecificMentalDisorders)
                    .ThenInclude(md => md.MentalDisorder)
                .Include(m => m.MedicalHistory)
                    .ThenInclude(mh => mh.PhysicalSymptoms)
                .Include(m => m.SpecificMentalDisorders)
                .ThenInclude(md => md.MentalDisorder)
                .FirstOrDefaultAsync(m => m.Id == request.MedicalRecordId, cancellationToken);

            if (medicalRecord is null) throw new KeyNotFoundException($"Không tìm thấy hồ sơ y tế {request.MedicalRecordId}");

            var medicalRecordDto = medicalRecord.Adapt<MedicalRecordDto>();

            return new GetMedicalRecordResult(medicalRecordDto);
        }
    }
}
