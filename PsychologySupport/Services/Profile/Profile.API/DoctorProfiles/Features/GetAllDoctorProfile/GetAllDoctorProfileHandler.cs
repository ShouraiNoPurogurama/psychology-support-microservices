using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetAllDoctorProfile;

public record GetAllDoctorProfilesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllDoctorProfilesResult>;

public record GetAllDoctorProfilesResult(IEnumerable<DoctorProfileDto> DoctorProfileDtos);

public class GetAllDoctorProfilesHandler : IQueryHandler<GetAllDoctorProfilesQuery, GetAllDoctorProfilesResult>
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

        var doctorProfiles = await _context.DoctorProfiles
            .Include(d => d.Specialties)
            .OrderByDescending(d => d.FullName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.MedicalRecords)
            .ToListAsync(cancellationToken);

        var result = doctorProfiles.Adapt<IEnumerable<DoctorProfileDto>>();

        return new GetAllDoctorProfilesResult(result);
    }
}