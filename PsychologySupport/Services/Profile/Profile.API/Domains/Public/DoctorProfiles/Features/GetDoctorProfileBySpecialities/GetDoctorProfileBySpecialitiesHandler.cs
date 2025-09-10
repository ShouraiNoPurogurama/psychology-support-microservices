using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.DoctorProfiles.Dtos;

namespace Profile.API.Domains.Public.DoctorProfiles.Features.GetDoctorProfileBySpecialities;

public record GetDoctorProfileBySpecialitiesQuery(string Specialties, PaginationRequest Request)
    : IQuery<GetDoctorProfileBySpecialitiesResult>;

public record GetDoctorProfileBySpecialitiesResult(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetDoctorProfileBySpecialitiesHandler(ProfileDbContext context)
    : IQueryHandler<GetDoctorProfileBySpecialitiesQuery, GetDoctorProfileBySpecialitiesResult>
{
    public async Task<GetDoctorProfileBySpecialitiesResult> Handle(GetDoctorProfileBySpecialitiesQuery request,
        CancellationToken cancellationToken)
    {
        var specialties = request.Specialties
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        var validSpecialtiesQuery = context.Specialties.AsQueryable();

        List<string> validSpecialties = [];
        
        foreach (var specialty in specialties)
        {
            var matchingSpecialties = await validSpecialtiesQuery
                .Where(s => EF.Functions.ILike(s.Name, $"%{specialty}%"))
                .Select(s => s.Name)
                .ToListAsync(cancellationToken);
            
            validSpecialties.AddRange(matchingSpecialties);
        }

        var doctorProfilesQuery = context.DoctorProfiles
            .Include(d => d.Specialties)
            .Where(d => d.Specialties.Any(s => validSpecialties.Contains(s.Name)))
            .AsQueryable();

        var totalCount = await doctorProfilesQuery.LongCountAsync(cancellationToken);

        var pageSize = Math.Max(1, request.Request.PageSize);
        var pageIndex = Math.Max(1, request.Request.PageIndex);

        var doctorProfiles = await doctorProfilesQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<DoctorProfileDto>()
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetDoctorProfileBySpecialitiesResult(
            new PaginatedResult<DoctorProfileDto>(pageIndex, pageSize, totalCount, doctorProfiles));
    }
}