using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.MentalDisorders.Dtos;

namespace Profile.API.MentalDisorders.Features.GetAllSpecificMentalDisorders
{
    public record GetAllSpecificMentalDisordersQuery(PaginationRequest PaginationRequest) : IQuery<GetAllSpecificMentalDisordersResult>;
    public record GetAllSpecificMentalDisordersResult(PaginatedResult<SpecificMentalDisorderDto> PaginatedResult);

    public class GetAllSpecificMentalDisordersHandler : IQueryHandler<GetAllSpecificMentalDisordersQuery, GetAllSpecificMentalDisordersResult>
    {
        private readonly ProfileDbContext _context;

        public GetAllSpecificMentalDisordersHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllSpecificMentalDisordersResult> Handle(GetAllSpecificMentalDisordersQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PaginationRequest.PageSize;
            var pageIndex = request.PaginationRequest.PageIndex;

            var specificMentalDisorders = await _context.SpecificMentalDisorders
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(s => s.MentalDisorder)
                .ToListAsync(cancellationToken);

            var totalCount = await _context.SpecificMentalDisorders.LongCountAsync(cancellationToken);

            var result = new PaginatedResult<SpecificMentalDisorderDto>(pageIndex, pageSize, totalCount,
                specificMentalDisorders.Adapt<IEnumerable<SpecificMentalDisorderDto>>());

            return new GetAllSpecificMentalDisordersResult(result);
        }
    }
}
