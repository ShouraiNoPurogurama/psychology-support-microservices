using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.DoctorProfiles.Dtos;

namespace Profile.API.Domains.Public.DoctorProfiles.Features.GetAllSpecialty
{
    public record GetAllSpecialtiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllSpecialtiesResult>;

    public record GetAllSpecialtiesResult(IEnumerable<SpecialtyDto> Specialties);
    public class GetAllSpecialtyHandler : IQueryHandler<GetAllSpecialtiesQuery, GetAllSpecialtiesResult>
    {
        private readonly ProfileDbContext _context;

        public GetAllSpecialtyHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllSpecialtiesResult> Handle(GetAllSpecialtiesQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PaginationRequest.PageSize;
            var pageIndex = request.PaginationRequest.PageIndex;

            var specialties = await _context.Specialties
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SpecialtyDto(s.Id, s.Name))
                .ToListAsync(cancellationToken);

            return new GetAllSpecialtiesResult(specialties);
        }
    }
}
