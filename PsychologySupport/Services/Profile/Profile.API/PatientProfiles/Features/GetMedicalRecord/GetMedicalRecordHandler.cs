using Mapster;
using Profile.API.MentalDisorders.Dtos;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetMedicalRecord
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

            if (medicalRecord is null) throw new KeyNotFoundException("Medical record not found.");

            var medicalRecordDto = new MedicalRecordDto(
                medicalRecord.Id,
                medicalRecord.PatientProfileId,
                medicalRecord.DoctorProfileId,
                medicalRecord.MedicalHistory?.Adapt<MedicalHistoryDto>(),
                medicalRecord.Notes,
                medicalRecord.Status,
                medicalRecord.SpecificMentalDisorders
                    .Select(md => new SpecificMentalDisorderDto(md.Id, md.MentalDisorder.Name, md.Name, md.Description))
                    .ToList()
            );

            return new GetMedicalRecordResult(medicalRecordDto);
        }
    }
}
