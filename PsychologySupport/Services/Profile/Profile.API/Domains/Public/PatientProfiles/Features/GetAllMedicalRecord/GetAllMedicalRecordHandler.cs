using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.PatientProfiles.Dtos;
using Profile.API.Domains.Public.PatientProfiles.Enum;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsQuery(
    Guid? PatientId = null, 
    Guid? DoctorId = null, 
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "", 
    string? SortBy = "CreatedAt",
    string? SortOrder = "asc",
    MedicalRecordStatus? Status = null) : IRequest<GetAllMedicalRecordsResult>;// fiter Processing,Done

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
  
        IQueryable<MedicalRecord> query = _context.MedicalRecords.AsQueryable();


        if (request.PatientId.HasValue)
        {
            query = query.Where(m => m.PatientProfileId == request.PatientId);
        }


        if (request.DoctorId.HasValue)
        {
            query = query.Where(m => m.DoctorProfileId == request.DoctorId);
        }


        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            if (Guid.TryParse(request.Search, out Guid searchId))
            {
                query = query.Where(m => m.DoctorProfileId == searchId || m.PatientProfileId == searchId);
            }
        }

     
        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.SortBy) && request.SortBy.ToLower() == "createdat")
        {
            query = request.SortOrder.ToLower() == "asc"
                ? query.OrderBy(m => m.CreatedAt)
                : query.OrderByDescending(m => m.CreatedAt);
        }

  
        var totalRecords = await query.CountAsync(cancellationToken);

 
        var medicalRecords = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);


        var medicalRecordsDto = medicalRecords.Adapt<IEnumerable<MedicalRecordDto>>();


        var paginatedResult = new PaginatedResult<MedicalRecordDto>(
            request.PageIndex,
            request.PageSize,
            totalRecords,
            medicalRecordsDto
        );

        return new GetAllMedicalRecordsResult(paginatedResult);
    }
}