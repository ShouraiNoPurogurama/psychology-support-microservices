using BuildingBlocks.Messaging.Events.Scheduling;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.DoctorProfiles.Models;

namespace Profile.API.DoctorProfiles.Features.GetAllDoctorProfile;

public record GetAllDoctorProfilesQuery(
    [FromQuery] int PageIndex,
    [FromQuery] int PageSize,
    [FromQuery] string? Search = "", // FullName
    [FromQuery] string? SortBy = "rating", // sort Rating,YearsOfExperience
    [FromQuery] string? SortOrder = "asc", // asc or desc
    [FromQuery] Guid? Specialties = null, // Specialties.Id - filter
    [FromQuery] DateTime? StartDate = null,
    [FromQuery] DateTime? EndDate = null
) : IRequest<GetAllDoctorProfilesResult>;

public record GetAllDoctorProfilesResult(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetAllDoctorProfilesHandler : IRequestHandler<GetAllDoctorProfilesQuery, GetAllDoctorProfilesResult>
{
    private readonly ProfileDbContext _context;
    private readonly IRequestClient<GetDoctorAvailabilityRequest> _availabilityClient;

    public GetAllDoctorProfilesHandler(ProfileDbContext context, IRequestClient<GetDoctorAvailabilityRequest> availabilityClient)
    {
        _context = context;
        _availabilityClient = availabilityClient;
    }

    public async Task<GetAllDoctorProfilesResult> Handle(GetAllDoctorProfilesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Max(1, request.PageSize);
        var pageIndex = Math.Max(1, request.PageIndex);

        IQueryable<DoctorProfile> query = _context.DoctorProfiles
            .Include(d => d.Specialties)
            .Include(d => d.MedicalRecords);

        //  Filter by Specialty.Id
        if (request.Specialties != null)
        {
            query = query.Where(d => d.Specialties.Any(s => request.Specialties == s.Id));
        }

        //  Search 
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(d => d.FullName.Contains(request.Search));
        }

        // Filter by Date
        if (request.StartDate != null && request.EndDate != null)
        {
            query = await FilterAvailableDoctors(query, request.StartDate.Value, request.EndDate.Value);
        }

        //  Sorting
        query = ApplySorting(query, request);

        var totalRecords = await query.CountAsync(cancellationToken);

        var doctorProfiles = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<DoctorProfileDto>(
            pageIndex,
            pageSize,
            totalRecords,
            doctorProfiles.Adapt<IEnumerable<DoctorProfileDto>>()
        );

        return new GetAllDoctorProfilesResult(paginatedResult);
    }

    private async Task<IQueryable<DoctorProfile>> FilterAvailableDoctors(IQueryable<DoctorProfile> doctors, DateTime startDate, DateTime endDate)
    {
        var doctorIds = await doctors.Select(d => d.Id).ToListAsync(); 

        var response = await _availabilityClient.GetResponse<GetDoctorAvailabilityResponse>(
            new GetDoctorAvailabilityRequest(doctorIds, startDate, endDate)
        );

        var availableDoctorIds = response.Message.AvailabilityMap
            .Where(kv => kv.Value) // check true
            .Select(kv => kv.Key)
            .ToList();

        return doctors.Where(d => availableDoctorIds.Contains(d.Id)); 
    }



    private static IQueryable<DoctorProfile> ApplySorting(IQueryable<DoctorProfile> query, GetAllDoctorProfilesQuery request)
    {
        bool isAscending = request.SortOrder?.ToLower() != "desc";

        return request.SortBy?.ToLower() switch
        {
            "rating" => isAscending ? query.OrderBy(d => d.Rating) : query.OrderByDescending(d => d.Rating),
            "yearsofexperience" => isAscending ? query.OrderBy(d => d.YearsOfExperience) : query.OrderByDescending(d => d.YearsOfExperience),
            _ => query // Default case
        };
    }
}

