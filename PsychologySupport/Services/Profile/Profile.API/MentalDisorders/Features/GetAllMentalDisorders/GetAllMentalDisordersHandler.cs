using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using Mapster;
using Profile.API.Extensions;
using Profile.API.MentalDisorders.Dtos;
using Profile.API.MentalDisorders.Models;

namespace Profile.API.MentalDisorders.Features.GetAllMentalDisorders
{
    public record GetAllMentalDisordersQuery(PaginationRequest PaginationRequest) : IQuery<GetAllMentalDisordersResult>;

    public record GetAllMentalDisordersResult(PaginatedResult<MentalDisorderDto> PaginatedResult);

    public class GetAllMentalDisordersHandler : IQueryHandler<GetAllMentalDisordersQuery, GetAllMentalDisordersResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

        public GetAllMentalDisordersHandler(ProfileDbContext context, IRequestClient<GetTranslatedDataRequest> translationClient)
        {
            _context = context;
            _translationClient = translationClient;
        }

        public async Task<GetAllMentalDisordersResult> Handle(GetAllMentalDisordersQuery request,
            CancellationToken cancellationToken)
        {
            var pageSize = request.PaginationRequest.PageSize;
            var pageIndex = request.PaginationRequest.PageIndex;

            var mentalDisorders = await _context.MentalDisorders
                .OrderBy(m => m.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.SpecificMentalDisorders)
                .ProjectToType<MentalDisorderDto>()
                .ToListAsync(cancellationToken: cancellationToken);

            var translatedMentalDisorders = await mentalDisorders.TranslateEntitiesAsync(
                nameof(MentalDisorder),
                _translationClient,
                s => s.Id.ToString(),
                cancellationToken,
                s => s.Name,
                s => s.Description
            );


            var totalCount = await _context.MentalDisorders.LongCountAsync(cancellationToken);

            var result = new PaginatedResult<MentalDisorderDto>(pageIndex, pageSize, totalCount,
                translatedMentalDisorders);

            return new GetAllMentalDisordersResult(result);
        }
    }
}