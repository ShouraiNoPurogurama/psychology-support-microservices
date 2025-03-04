using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.MentalDisorders.Dtos;

namespace Profile.API.MentalDisorders.Features.GetAllMentalDisorders
{
    public record GetAllMentalDisordersQuery(PaginationRequest PaginationRequest) : IQuery<GetAllMentalDisordersResult>;

    public record GetAllMentalDisordersResult(PaginatedResult<MentalDisorderDto> PaginatedResult);

    public class GetAllMentalDisordersHandler : IQueryHandler<GetAllMentalDisordersQuery, GetAllMentalDisordersResult>
    {
        private readonly ProfileDbContext _context;

        public GetAllMentalDisordersHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllMentalDisordersResult> Handle(GetAllMentalDisordersQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PaginationRequest.PageSize;
            var pageIndex = request.PaginationRequest.PageIndex;

            var mentalDisorders = await _context.MentalDisorders
                .OrderBy(m => m.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.SpecificMentalDisorders)
                .ToListAsync(cancellationToken);

            var totalCount = await _context.MentalDisorders.LongCountAsync(cancellationToken);

            var result = new PaginatedResult<MentalDisorderDto>(pageIndex, pageSize, totalCount,
                mentalDisorders.Adapt<IEnumerable<MentalDisorderDto>>());

            return new GetAllMentalDisordersResult(result);
        }
    }
}