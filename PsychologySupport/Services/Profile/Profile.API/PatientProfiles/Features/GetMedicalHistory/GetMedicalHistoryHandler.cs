using Microsoft.EntityFrameworkCore;
using Profile.API.MentalDisorders.Dtos;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.API.PatientProfiles.Features.GetMedicalHistory
{
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
                .Include(p => p.MedicalHistory)
                    .ThenInclude(mh => mh.PhysicalSymptoms)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient is null)
            {
                throw new KeyNotFoundException("Patient not found.");
            }

            if (patient.MedicalHistory is null)
            {
                throw new KeyNotFoundException("Medical history not found.");
            }

            var medicalHistoryDto = new MedicalHistoryDto(
                patient.MedicalHistory.Description,
                patient.MedicalHistory.DiagnosedAt,
                patient.MedicalHistory.SpecificMentalDisorders
               .Select(md => new SpecificMentalDisorderDto(md.Id, md.MentalDisorderId, md.Name, md.Description))
               .ToList(),
               patient.MedicalHistory.PhysicalSymptoms
               .Select(ps => new PhysicalSymptomDto(ps.Id, ps.Name, ps.Description)) 
               .ToList()

            );

            return new GetMedicalHistoryResult(medicalHistoryDto);
        }
    }
}
