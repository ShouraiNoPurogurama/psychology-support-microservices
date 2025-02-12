using BuildingBlocks.CQRS;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pricing.API.Data;
using Pricing.API.Models;

namespace Pricing.API.Features.ExperiencePriceRanges.GetExperiencePriceRanges;

public record GetExperiencePriceRangesQuery(int PageNumber, int PageSize) : IQuery<GetExperiencePriceRangesResult>;

public record GetExperiencePriceRangesResult(IEnumerable<ExperiencePriceRange> ExperiencePriceRanges, int TotalCount);

public class GetExperiencePriceRangesHandler : IQueryHandler<GetExperiencePriceRangesQuery, GetExperiencePriceRangesResult>
{
    private readonly PricingDbContext _context;

    public GetExperiencePriceRangesHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<GetExperiencePriceRangesResult> Handle(GetExperiencePriceRangesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var totalCount = await _context.ExperiencePriceRanges.CountAsync(cancellationToken);

        var priceRanges = await _context.ExperiencePriceRanges
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetExperiencePriceRangesResult(priceRanges.Adapt<IEnumerable<ExperiencePriceRange>>(), totalCount);
    }
}
