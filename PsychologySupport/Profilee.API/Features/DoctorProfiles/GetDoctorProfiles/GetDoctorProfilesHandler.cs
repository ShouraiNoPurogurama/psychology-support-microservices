using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Profilee.API.Data;
using Profilee.API.Models;

namespace Profilee.API.Features.DoctorProfiles.GetDoctorProfiles
{
    public record GetDoctorProfilesQuery() : IQuery<GetDoctorProfilesResult>;

    public record GetDoctorProfilesResult(IEnumerable<DoctorProfile> DoctorProfiles) ;
    public class GetDoctorProfilesHandler : IQueryHandler<GetDoctorProfilesQuery, GetDoctorProfilesResult>
    {
        private readonly ProfileDbContext dbContext;

        public GetDoctorProfilesHandler(ProfileDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<GetDoctorProfilesResult> Handle(GetDoctorProfilesQuery request, CancellationToken cancellationToken)
        {
            var doctors = await dbContext.DoctorProfiles.ToListAsync(cancellationToken);

            return new GetDoctorProfilesResult(doctors);
        }
    }
}
