using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Exceptions;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Enum;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsQuery(
    [FromQuery] Guid? PatientId = null, 
    [FromQuery] Guid? DoctorId = null, 
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = "", 
    [FromQuery] string? SortBy = "CreatedAt",
    [FromQuery] string? SortOrder = "asc",
    [FromQuery] MedicalRecordStatus? Status = null) : IRequest<GetAllMedicalRecordsResult>;// fiter Processing,Done

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