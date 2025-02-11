using BuildingBlocks.CQRS;
using Mapster;
using Pricing.API.Data;
using Pricing.API.Dtos;
using Pricing.API.Exceptions;

namespace Pricing.API.Features.ExperiencePriceRanges.UpdateExperiencePriceRange;

public record UpdateExperiencePriceRangeCommand(ExperiencePriceRangeDto ExperiencePriceRange) : ICommand<UpdateExperiencePriceRangeResult>;

public record UpdateExperiencePriceRangeResult(bool IsSuccess);

public class UpdateExperiencePriceRangeHandler : ICommandHandler<UpdateExperiencePriceRangeCommand, UpdateExperiencePriceRangeResult>
{
    private readonly PricingDbContext _context;

    public UpdateExperiencePriceRangeHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateExperiencePriceRangeResult> Handle(UpdateExperiencePriceRangeCommand request, CancellationToken cancellationToken)
    {
        var existingRange = await _context.ExperiencePriceRanges.FindAsync(request.ExperiencePriceRange.Id)
                            ?? throw new PricingNotFoundException("Experience Price Range", request.ExperiencePriceRange.Id);

        existingRange = request.ExperiencePriceRange.Adapt(existingRange);

        _context.Update(existingRange);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateExperiencePriceRangeResult(result);
    }
}
