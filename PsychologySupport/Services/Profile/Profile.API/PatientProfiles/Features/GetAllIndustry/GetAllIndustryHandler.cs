using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetAllIndustry
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

        public GetAllIndustryHandler(ProfileDbContext context)
        {
            _context = context;
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

            var itemDtos = items.Adapt<IEnumerable<IndustryDto>>();

            var result = new PaginatedResult<IndustryDto>(
                request.PageIndex,
                request.PageSize,
                total,
                itemDtos
            );

            return new GetAllIndustryResult(result);
        }
    }
}
