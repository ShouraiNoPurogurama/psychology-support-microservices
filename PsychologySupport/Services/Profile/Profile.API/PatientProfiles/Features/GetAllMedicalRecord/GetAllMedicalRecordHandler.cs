using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.Exceptions;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsQuery(Guid PatientId, PaginationRequest PaginationRequest) : IRequest<GetAllMedicalRecordsResult>;

public record GetAllMedicalRecordsResult(PaginatedResult<MedicalRecordDto> MedicalRecords);

public class GetAllMedicalRecordsHandler : IRequestHandler<GetAllMedicalRecordsQuery, GetAllMedicalRecordsResult>
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
            throw new ProfileNotFoundException("Patient", request.PatientId);

        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var totalRecords = patient.MedicalRecords.Count;
        var medicalRecords = patient.MedicalRecords
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Adapt<IEnumerable<MedicalRecordDto>>();

        var paginatedResult = new PaginatedResult<MedicalRecordDto>(
            pageIndex + 1,
            pageSize,
            totalRecords,
            medicalRecords
        );

        return new GetAllMedicalRecordsResult(paginatedResult);
    }
}