using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.MentalDisorders.Dtos;
using Profile.API.Extensions;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.MentalDisorders.Features.GetAllSpecificMentalDisorders
{
    public record GetAllSpecificMentalDisordersQuery(
        PaginationRequest PaginationRequest,
        string? Search = "") : IQuery<GetAllSpecificMentalDisordersResult>;

    public record GetAllSpecificMentalDisordersResult(PaginatedResult<SpecificMentalDisorderDto> SpecificMentalDisorder);

    public class GetAllSpecificMentalDisordersHandler : IQueryHandler<GetAllSpecificMentalDisordersQuery,
        GetAllSpecificMentalDisordersResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;


        public GetAllSpecificMentalDisordersHandler(ProfileDbContext context,
            IRequestClient<GetTranslatedDataRequest> translationClient)
        {
            _context = context;
            _translationClient = translationClient;
        }

        public async Task<GetAllSpecificMentalDisordersResult> Handle(GetAllSpecificMentalDisordersQuery request,
            CancellationToken cancellationToken)
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
                .ProjectToType<SpecificMentalDisorderDto>()
                .ToListAsync(cancellationToken);

            var translatedSpecificMentalDisorders = await specificMentalDisorders.TranslateEntitiesAsync(
                nameof(SpecificMentalDisorder),
                _translationClient,
                s => s.Id.ToString(),
                cancellationToken,
                s => s.Name,
                s => s.Description,
                s => s.MentalDisorderName
            );
            
            var result = new PaginatedResult<SpecificMentalDisorderDto>(
                pageIndex, pageSize, totalCount, translatedSpecificMentalDisorders);

            return new GetAllSpecificMentalDisordersResult(result);
        }
    }
}