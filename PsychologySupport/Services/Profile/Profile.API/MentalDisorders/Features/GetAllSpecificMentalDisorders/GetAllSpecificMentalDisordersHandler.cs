using BuildingBlocks.Pagination;
using Mapster;
using Profile.API.MentalDisorders.Dtos;
using Profile.API.MentalDisorders.Models;

namespace Profile.API.MentalDisorders.Features.GetAllSpecificMentalDisorders
{
    public record GetAllSpecificMentalDisordersQuery(
    PaginationRequest PaginationRequest,
    string? Search = "") : IQuery<GetAllSpecificMentalDisordersResult>;

    public record GetAllSpecificMentalDisordersResult(PaginatedResult<SpecificMentalDisorderDto> SpecificMentalDisorder);

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
            var search = request.Search?.Trim().ToLower();

            IQueryable<SpecificMentalDisorder> query = _context.SpecificMentalDisorders
                .Include(s => s.MentalDisorder);

            // 🔍 Tìm kiếm theo Name hoặc MentalDisorder.Name nếu có
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    s.MentalDisorder.Name.ToLower().Contains(search));
            }

            var totalCount = await query.LongCountAsync(cancellationToken);

            var specificMentalDisorders = await query
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<SpecificMentalDisorderDto>(
                pageIndex, pageSize, totalCount, specificMentalDisorders.Adapt<IEnumerable<SpecificMentalDisorderDto>>());

            return new GetAllSpecificMentalDisordersResult(result);
        }
    }
}
