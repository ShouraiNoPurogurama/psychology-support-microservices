using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Profile.API.Data;
using Microsoft.EntityFrameworkCore;
using Profile.API.DoctorProfiles.Models;

namespace Profile.API.DoctorProfiles.Features.GetDoctorProfiles
{
    public record GetDoctorProfilesQuery(PaginationRequest Pagination) : IQuery<GetDoctorProfilesResult>;

    public record GetDoctorProfilesResult(PaginatedResult<DoctorProfile> DoctorProfiles);

    public class GetDoctorProfilesHandler : IQueryHandler<GetDoctorProfilesQuery, GetDoctorProfilesResult>
    {
        private readonly ProfileDbContext _dbContext;

        public GetDoctorProfilesHandler(ProfileDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDoctorProfilesResult> Handle(GetDoctorProfilesQuery request, CancellationToken cancellationToken)
        {
            var skip = (request.Pagination.PageIndex - 1) * request.Pagination.PageSize;
            var totalCount = await _dbContext.DoctorProfiles.CountAsync(cancellationToken);

            var doctorProfiles = await _dbContext.DoctorProfiles
                .Skip(skip)
                .Take(request.Pagination.PageSize)
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<DoctorProfile>(
                request.Pagination.PageIndex,
                request.Pagination.PageSize,
                totalCount,
                doctorProfiles
            );

            return new GetDoctorProfilesResult(paginatedResult);
        }
    }
}
