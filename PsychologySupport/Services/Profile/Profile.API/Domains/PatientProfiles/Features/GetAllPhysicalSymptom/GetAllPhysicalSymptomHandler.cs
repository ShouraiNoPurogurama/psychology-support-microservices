using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Models;

namespace Profile.API.Domains.PatientProfiles.Features.GetAllPhysicalSymptom
{
    public record GetAllPhysicalSymptomQuery(
    int PageIndex,
    int PageSize,
    string? Search = "") : IQuery<GetAllPhysicalSymptomResult>;

    public record GetAllPhysicalSymptomResult(PaginatedResult<PhysicalSymptomDto> PhysicalSymptom);

    public class GetAllPhysicalSymptomHandler : IQueryHandler<GetAllPhysicalSymptomQuery, GetAllPhysicalSymptomResult>
    {
        private readonly ProfileDbContext _context;

        public GetAllPhysicalSymptomHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllPhysicalSymptomResult> Handle(GetAllPhysicalSymptomQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize;
            var pageIndex = request.PageIndex;
            var search = request.Search?.Trim().ToLower();

            //IQueryable<PhysicalSymptom> query = _context.PhysicalSymptoms;
            var query = _context.PhysicalSymptoms.Where(p => true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            List<PhysicalSymptom>? symptoms = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<PhysicalSymptomDto>(
                pageIndex, pageSize, totalCount, symptoms.Adapt<List<PhysicalSymptomDto>>());

            return new GetAllPhysicalSymptomResult(result);
        }
    }
}
