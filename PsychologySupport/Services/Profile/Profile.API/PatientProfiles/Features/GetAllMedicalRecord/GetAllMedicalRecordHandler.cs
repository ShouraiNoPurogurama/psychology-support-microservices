using BuildingBlocks.CQRS;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Profile.API.PatientProfiles.Models;
using System.Threading;
using System.Threading.Tasks;

public record GetAllMedicalRecordsQuery(Guid PatientId, int PageNumber, int PageSize) : IQuery<GetAllMedicalRecordsResult>;

public record GetAllMedicalRecordsResult(IEnumerable<MedicalRecord> MedicalRecords, int TotalRecords);
public class GetAllMedicalRecordsHandler : IQueryHandler<GetAllMedicalRecordsQuery, GetAllMedicalRecordsResult>
{
    private readonly ProfileDbContext _context;

    public GetAllMedicalRecordsHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllMedicalRecordsResult> Handle(GetAllMedicalRecordsQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.MedicalRecords)
                .ThenInclude(m => m.SpecificMentalDisorders)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

        if (patient is null)
        {
            throw new KeyNotFoundException("Patient not found.");
        }

        var totalRecords = patient.MedicalRecords.Count;
        var medicalRecords = patient.MedicalRecords
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Adapt<IEnumerable<MedicalRecord>>();

        return new GetAllMedicalRecordsResult(medicalRecords, totalRecords);
    }
}
