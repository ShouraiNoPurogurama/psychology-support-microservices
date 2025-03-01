using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetAllDoctorProfile;

public record GetAllDoctorProfilesQuery(PaginationRequest PaginationRequest) : IRequest<GetAllDoctorProfilesResult>;

public record GetAllDoctorProfilesResult(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetAllDoctorProfilesHandler : IRequestHandler<GetAllDoctorProfilesQuery, GetAllDoctorProfilesResult>
{
    private readonly ProfileDbContext _context;

    public GetAllDoctorProfilesHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllDoctorProfilesResult> Handle(GetAllDoctorProfilesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(1, request.PaginationRequest.PageIndex);

        var totalRecords = await _context.DoctorProfiles.CountAsync(cancellationToken);

        var doctorProfiles = await _context.DoctorProfiles
            .Include(d => d.Specialties)
            .Include(d => d.MedicalRecords)
            .OrderByDescending(d => d.FullName)
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
}