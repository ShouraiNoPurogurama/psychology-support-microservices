using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllPatientProfilesResult>;

public record GetAllPatientProfilesResult(IEnumerable<GetPatientProfileDto> PatientProfileDtos);

public class GetAllPatientProfilesHandler : IQueryHandler<GetAllPatientProfilesQuery, GetAllPatientProfilesResult>
{
    private readonly ProfileDbContext _context;

    public GetAllPatientProfilesHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllPatientProfilesResult> Handle(GetAllPatientProfilesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = request.PaginationRequest.PageIndex;

        var patientProfiles = await _context.PatientProfiles
            .OrderByDescending(p => p.FullName)
            .Skip(pageIndex - 1)
            .Take(pageSize)
            .Include(p => p.MedicalRecords)
                .ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalHistory)
                .ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalHistory)
                .ThenInclude(m => m.PhysicalSymptoms)
            .ToListAsync(cancellationToken);

        var result = patientProfiles.Adapt<IEnumerable<GetPatientProfileDto>>();

        return new GetAllPatientProfilesResult(result);
    }
}