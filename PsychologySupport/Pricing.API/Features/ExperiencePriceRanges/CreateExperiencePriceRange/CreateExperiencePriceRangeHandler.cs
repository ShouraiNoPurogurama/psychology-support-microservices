using BuildingBlocks.CQRS;
using Mapster;
using Pricing.API.Data;
using Pricing.API.Models;

namespace Pricing.API.Features.ExperiencePriceRanges.CreateExperiencePriceRange;

public record CreateExperiencePriceRangeCommand(ExperiencePriceRange ExperiencePriceRange) : ICommand<CreateExperiencePriceRangeResult>;

public record CreateExperiencePriceRangeResult(Guid Id);

public class CreateExperiencePriceRangeHandler : ICommandHandler<CreateExperiencePriceRangeCommand, CreateExperiencePriceRangeResult>
{
    private readonly PricingDbContext _context;

    public CreateExperiencePriceRangeHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<CreateExperiencePriceRangeResult> Handle(CreateExperiencePriceRangeCommand request, CancellationToken cancellationToken)
    {
        var experiencePriceRange = request.ExperiencePriceRange.Adapt<ExperiencePriceRange>();

        _context.ExperiencePriceRanges.Add(experiencePriceRange);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateExperiencePriceRangeResult(experiencePriceRange.Id);
    }
}

