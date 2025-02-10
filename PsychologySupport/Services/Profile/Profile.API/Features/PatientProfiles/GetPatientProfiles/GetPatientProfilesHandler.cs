using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Profile.API.Data;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.GetPatientProfiles;

public record GetPatientProfilesQuery() : IQuery<GetPatientProfilesResult>;
public record GetPatientProfilesResult(IEnumerable<PatientProfile> PatientProfiles);

public class GetPatientProfilesHandler : IQueryHandler<GetPatientProfilesQuery, GetPatientProfilesResult>
{
    private readonly ProfileDbContext _dbContext;

    public GetPatientProfilesHandler(ProfileDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<GetPatientProfilesResult> Handle(GetPatientProfilesQuery request, CancellationToken cancellationToken)
    {
        var patients = await _dbContext.PatientProfiles.ToListAsync(cancellationToken);

        return new GetPatientProfilesResult(patients);
    }
}