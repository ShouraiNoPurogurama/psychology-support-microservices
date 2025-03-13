using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Exceptions;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Enum;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetAllMedicalRecord;

public record GetAllMedicalRecordsQuery(
        [FromRoute] Guid PatientId,
        [FromQuery] int PageIndex,
        [FromQuery] int PageSize,
        [FromQuery] string? Search = "", // DoctorProfileId
        [FromQuery] string? SortBy = "CreatedAt", // sort CreatedAt
        [FromQuery] string? SortOrder = "asc", // asc or desc
        [FromQuery] MedicalRecordStatus? Status = null): IRequest<GetAllMedicalRecordsResult>; // fiter Processing,Done

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

        var pageSize = request.PageSize;
        var pageIndex = Math.Max(0, request.PageIndex - 1);

        IQueryable<MedicalRecord> query = _context.MedicalRecords
            .Where(m => m.PatientProfileId == request.PatientId);


        // search  
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(m =>
                     m.DoctorProfileId.ToString() == request.Search 
            );
        }

        // Filter by MedicalRecordStatus 
        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status.Value);
        }

        // Sorting CreatedAt
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            if (request.SortBy.ToLower() == "createdat")
            {
                query = request.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(m => m.CreatedAt)
                    : query.OrderByDescending(m => m.CreatedAt);
            }

        }

        var totalRecords = await query.CountAsync(cancellationToken);

      
        var medicalRecords = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var medicalRecordsDto = medicalRecords.Adapt<IEnumerable<MedicalRecordDto>>();

        var paginatedResult = new PaginatedResult<MedicalRecordDto>(
            pageIndex + 1, 
            pageSize,
            totalRecords,
            medicalRecordsDto
        );

        return new GetAllMedicalRecordsResult(paginatedResult);
    }
}