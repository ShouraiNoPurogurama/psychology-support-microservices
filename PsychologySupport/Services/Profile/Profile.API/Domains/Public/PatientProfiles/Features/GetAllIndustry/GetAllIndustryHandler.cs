using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Public.PatientProfiles.Dtos;
using Profile.API.Extensions;
using Profile.API.Models.Public;
using Translation.API.Protos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetAllIndustry
{
    public record GetAllIndustryQuery(
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = "",
    [FromQuery] string? SortBy = "Name",
    [FromQuery] string? SortOrder = "asc"
    ) : IRequest<GetAllIndustryResult>;

    public record GetAllIndustryResult(PaginatedResult<IndustryDto> Industries);
    public class GetAllIndustryHandler : IRequestHandler<GetAllIndustryQuery, GetAllIndustryResult>
    {
        private readonly ProfileDbContext _context;
        private readonly TranslationService.TranslationServiceClient _translationClient;

        public GetAllIndustryHandler(ProfileDbContext context, TranslationService.TranslationServiceClient translationClient)
        {
            _context = context;
            _translationClient = translationClient;
        }

        public async Task<GetAllIndustryResult> Handle(GetAllIndustryQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Industry> query = _context.Industries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(i => i.IndustryName.Contains(request.Search));
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy) && request.SortBy.ToLower() == "name")
            {
                query = request.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.IndustryName)
                    : query.OrderByDescending(i => i.IndustryName);
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var itemDtos = items.Adapt<List<IndustryDto>>();

            var translatedItems = await itemDtos.TranslateEntitiesAsync(nameof(Industry),
                _translationClient,
                i => i.Id.ToString(),
                cancellationToken,
                i => i.IndustryName
            );
            
            var result = new PaginatedResult<IndustryDto>(
                request.PageIndex,
                request.PageSize,
                total,
                translatedItems
            );

            return new GetAllIndustryResult(result);
        }
    }
}
