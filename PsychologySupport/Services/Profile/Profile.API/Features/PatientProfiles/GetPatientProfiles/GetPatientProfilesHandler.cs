using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Profile.API.Data;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.GetPatientProfiles;

public record GetPatientProfilesQuery(int PageNumber, int PageSize) : IQuery<GetPatientProfilesResult>;

public record GetPatientProfilesResult(IEnumerable<PatientProfile> PatientProfiles, int TotalCount);

public class GetPatientProfilesHandler : IQueryHandler<GetPatientProfilesQuery, GetPatientProfilesResult>
{
    private readonly ProfileDbContext _dbContext;

    public GetPatientProfilesHandler(ProfileDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetPatientProfilesResult> Handle(GetPatientProfilesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var totalCount = await _dbContext.PatientProfiles.CountAsync(cancellationToken);

        var patients = await _dbContext.PatientProfiles
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetPatientProfilesResult(patients, totalCount);
    }
}
