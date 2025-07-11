using BuildingBlocks.Pagination;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetPatientProfilesCreated;

public record GetPatientProfilesCreatedQuery(
    PaginationRequest Request,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IQuery<GetPatientProfilesCreatedResult>;

public record GetPatientProfilesCreatedResult(
    IEnumerable<GetCreatedPatientProfileDto> Datapoints
);

public class GetPatientProfilesCreatedHandler(ProfileDbContext dbContext)
    : IQueryHandler<GetPatientProfilesCreatedQuery, GetPatientProfilesCreatedResult>
{
    public async Task<GetPatientProfilesCreatedResult> Handle(GetPatientProfilesCreatedQuery request,
        CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddMonths(-11).Date;

        var profilesQuery = dbContext.PatientProfiles
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        var groupedProfiles = await profilesQuery
            .GroupBy(p => new { Year = p.CreatedAt!.Value.Year, Month = p.CreatedAt.Value.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                Profiles = g.OrderByDescending(p => p.CreatedAt).ToList()
            })
            .ToListAsync(cancellationToken);

        var datapoints = new List<GetCreatedPatientProfileDto>();

        foreach (var group in groupedProfiles.OrderBy(g => new DateTime(g.Year, g.Month, 1)))
        {
            var page = request.Request.PageIndex;
            var pageSize = request.Request.PageSize;

            var profilesPage = group.Profiles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var simplifiedProfiles = profilesPage.Select(p => new SimplifiedPatientProfileDto(
                        Id: p.Id,
                        FullName: p.FullName,
                        Gender: p.Gender,
                        BirthDate: p.BirthDate.GetValueOrDefault(),
                        CreatedAt: p.CreatedAt.GetValueOrDefault()
                    )
                )
                .ToList();

            var paginatedResult = new PaginatedResult<SimplifiedPatientProfileDto>
            (
                PageIndex: page,
                PageSize: pageSize,
                TotalCount: group.Count,
                Data: simplifiedProfiles
            );

            datapoints.Add(new GetCreatedPatientProfileDto(
                Date: new DateTime(group.Year, group.Month, 1),
                Profiles: paginatedResult
            ));
        }

        return new GetPatientProfilesCreatedResult(datapoints);
    }
}