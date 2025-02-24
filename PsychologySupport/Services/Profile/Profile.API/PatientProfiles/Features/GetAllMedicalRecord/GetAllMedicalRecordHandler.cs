using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsQuery(Guid PatientId, PaginationRequest PaginationRequest) : IQuery<GetAllMedicalRecordsResult>;

public record GetAllMedicalRecordsResult(IEnumerable<MedicalRecordDto> MedicalRecords, int TotalRecords);
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

        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(1, request.PaginationRequest.PageIndex);

        var totalRecords = patient.MedicalRecords.Count;
        var medicalRecords = patient.MedicalRecords
            .Skip((pageIndex - 1) * pageSize)  
            .Take(pageSize)
            .Adapt<IEnumerable<MedicalRecordDto>>();


        return new GetAllMedicalRecordsResult(medicalRecords, totalRecords);
    }
}