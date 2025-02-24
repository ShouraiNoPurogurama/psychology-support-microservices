using BuildingBlocks.CQRS;
using Mapster;
using Pricing.API.Data;
using Pricing.API.Exceptions;
using Pricing.API.Models;

namespace Pricing.API.Features.ExperiencePriceRanges.GetExperiencePriceRange;

public record GetExperiencePriceRangeQuery(Guid Id) : IQuery<GetExperiencePriceRangeResult>;

public record GetExperiencePriceRangeResult(ExperiencePriceRange ExperiencePriceRange);

public class GetExperiencePriceRangeHandler : IQueryHandler<GetExperiencePriceRangeQuery, GetExperiencePriceRangeResult>
{
    private readonly PricingDbContext _context;

    public GetExperiencePriceRangeHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<GetExperiencePriceRangeResult> Handle(GetExperiencePriceRangeQuery request, CancellationToken cancellationToken)
    {
        var experiencePriceRange = await _context.ExperiencePriceRanges.FindAsync(request.Id)
                                  ?? throw new PricingNotFoundException("Experience Price Range", request.Id);

        return new GetExperiencePriceRangeResult(experiencePriceRange.Adapt<ExperiencePriceRange>());
    }
}
